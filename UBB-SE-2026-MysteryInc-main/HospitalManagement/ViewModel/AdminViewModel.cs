using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using HospitalManagement.Service;
using HospitalManagement.View;
using HospitalManagement.View.DialogServiceAdmin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal partial class AdminViewModel : ObservableObject
{
    #region Variables

    private readonly IPatientService _patientService;
    private readonly IGhostService _ghostService;
    private readonly ITransplantService _transplantService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _currentView = "";

    [ObservableProperty]
    private bool _isArchivedMode;

    [ObservableProperty]
    private bool _isExorcismAlertVisible;

    [ObservableProperty]
    private Patient? _editingPatient;

    [ObservableProperty]
    private bool _noResultsFound;

    [ObservableProperty]
    private double? _minAge;

    [ObservableProperty]
    private double? _maxAge;

    [ObservableProperty]
    private object? _selectedSexFilter;

    [ObservableProperty]
    private string? _cnpError;

    [ObservableProperty]
    private string? _phoneError;

    [ObservableProperty]
    private string? _dobError;

    [ObservableProperty]
    private DateTime? _dateOfDeath;

    [ObservableProperty]
    private bool _isStatisticsVisible;

    #endregion Variables

    #region Navigation

    public Action? CloseAddPatientWindow { get; set; }

    partial void OnIsArchivedModeChanged(bool value) =>
        OnPropertyChanged(nameof(IsActiveMode));

    [RelayCommand]
    private static void NavigateHome()
    {
        MainWindow mainWindow = (Application.Current as App)!.Services
            .GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }

    [RelayCommand]
    private void ToggleStatistics() => IsStatisticsVisible = !IsStatisticsVisible;

    [RelayCommand]
    private void ExecuteSwitchToActive() => IsArchivedMode = false;

    [RelayCommand]
    private void NavigateToStatistics() => CurrentView = "Statistics";

    #endregion Navigation

    #region Patients

    public ObservableCollection<Patient> Patients { get; set; }

    public ObservableCollection<Patient> ArchivedPatients { get; set; }

    public Patient NewPatient { get; set; }

    private Patient? _selectedPatient;

    public Patient? SelectedPatient
    {
        get => _selectedPatient;

        set
        {
            _selectedPatient = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotDeceased));
            OnPropertyChanged(nameof(IsDeceased));

            if (_selectedPatient is not null)
            {
                EditingPatient = new Patient
                {
                    Id = _selectedPatient.Id,
                    FirstName = _selectedPatient.FirstName,
                    LastName = _selectedPatient.LastName,
                    Cnp = _selectedPatient.Cnp,
                    Dob = _selectedPatient.Dob,
                    Sex = _selectedPatient.Sex,
                    PhoneNo = _selectedPatient.PhoneNo,
                    EmergencyContact = _selectedPatient.EmergencyContact,
                    Dod = _selectedPatient.Dod,
                };
            }
        }
    }

    #endregion Patients

    #region State

    public bool IsActiveMode => !IsArchivedMode;

    public bool IsNotDeceased => SelectedPatient?.IsDeceased == false;

    public bool IsDeceased => SelectedPatient?.IsDeceased == true;

    #endregion State

    #region Filters

    private string? _searchQuery;

    public string? SearchQuery
    {
        get => _searchQuery;

        set
        {
            _searchQuery = value;
            OnPropertyChanged();
            SearchPatient();
        }
    }

    #endregion Filters

    #region Constructor

    public AdminViewModel()
    {
        _ghostService = (Application.Current as App)!.Services.GetRequiredService<IGhostService>();
        _patientService = (Application.Current as App)!.Services.GetRequiredService<IPatientService>();
        _transplantService = (Application.Current as App)!.Services.GetRequiredService<ITransplantService>();
        _dialogService = (Application.Current as App)!.Services.GetRequiredService<IDialogService>();

        Patients = [];
        ArchivedPatients = [];
        NewPatient = new Patient { Dob = DateTime.Today, };

        _ghostService.ExorcismTriggered += (s, e) => IsExorcismAlertVisible = true;
        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();

        LoadAllPatients();
        CurrentView = "AdminDashboard";
    }

    #endregion Constructor

    #region Methods

    [RelayCommand]
    public void GhostSighting() => _ghostService.SawAGhost();

    [RelayCommand]
    public void LoadAllPatients()
    {
        var emptyFilter = new PatientFilter();
        List<Patient> allPatients = _patientService.SearchPatients(emptyFilter);

        Patients.Clear();
        foreach (Patient patient in allPatients.Where(p => !p.IsArchived))
        {
            patient.PhoneNo = FormatPhoneNumber(patient.PhoneNo);
            patient.EmergencyContact = FormatPhoneNumber(patient.EmergencyContact);
            Patients.Add(patient);
        }
    }

    [RelayCommand]
    public void LoadArchivedPatients()
    {
        IsArchivedMode = true;
        var emptyFilter = new PatientFilter();
        List<Patient> allPatients = _patientService.SearchPatients(emptyFilter);

        ArchivedPatients.Clear();
        foreach (Patient patient in allPatients.Where(p => p.IsArchived))
        {
            patient.PhoneNo = FormatPhoneNumber(patient.PhoneNo);
            patient.EmergencyContact = FormatPhoneNumber(patient.EmergencyContact);
            ArchivedPatients.Add(patient);
        }
    }

    [RelayCommand]
    private void OpenPatientDetails()
    {
        if (SelectedPatient is null) return;

        IServiceProvider scope = (Application.Current as App)!.Services;
        PatientView patientWindow = scope.GetRequiredService<PatientView>();
        patientWindow.Initialize(SelectedPatient.Id, () => { });
        patientWindow.Activate();
    }

    public async Task AssignOrganDonorAsync(int transplantId, int donorId, float score, string donorName)
    {
        try
        {
            _transplantService.AssignDonor(transplantId, donorId, score);
            await _dialogService.ShowAlertAsync($"Successfully assigned organ from donor {donorName}.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error assigning organ: {ex.Message}");
        }
    }

    public async Task ProcessMedicalHistoryResultAsync(int patientId, MedicalHistory history, bool wasSkipped)
    {
        if (wasSkipped)
        {
            await _dialogService.ShowAlertAsync("You can add medical history later from the patient profile.");
            return;
        }

        if (history is null) return;

        try
        {
            history.PatientId = patientId;
            _patientService.CreateMedicalHistory(patientId, history);
            await _dialogService.ShowAlertAsync("Medical history saved successfully!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error saving medical history: {ex.Message}");
        }
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;

        phone = phone.Replace(" ", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal);

        if (!phone.StartsWith('0') || phone.Length != 10) return phone;

        return $"+40 {phone.Substring(1, 3)} {phone.Substring(4, 3)} {phone.Substring(7, 3)}";
    }

    [RelayCommand]
    private async Task AddPatientAsync()
    {
        Patient? patient = await _dialogService.ShowAddPatientDialogAsync();
        if (patient is null) return;

        try
        {
            patient.PhoneNo = FormatPhoneNumber(patient.PhoneNo);
            patient.EmergencyContact = FormatPhoneNumber(patient.EmergencyContact);
            Patients.Add(patient);

            MedicalHistoryEntry entry = await _dialogService.ShowMedicalHistoryAsync();
            await ProcessMedicalHistoryResultAsync(patient.Id, entry.History, entry.WasSkipped);
            await _dialogService.ShowAlertAsync("Patient added successfully.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ArchivePatientAsync()
    {
        if (SelectedPatient is null) return;

        bool isConfirmed = await (_dialogService.ShowConfirmAsync(
            $"Are you sure you want to archive {SelectedPatient.FirstName} {SelectedPatient.LastName}?",
            "Confirm Archive")
            ?? Task.FromResult(false));

        if (!isConfirmed) return;

        _patientService.ArchivePatient(SelectedPatient.Id);
        Patients.Remove(SelectedPatient);
        ArchivedPatients.Add(SelectedPatient);
    }

    [RelayCommand]
    private async Task DearchivePatientAsync()
    {
        if (SelectedPatient is null) return;

        if (SelectedPatient.IsDeceased)
        {
            await _dialogService.ShowAlertAsync("Cannot dearchive this patient. The record indicates the patient is deceased.");
            return;
        }

        _patientService.DearchivePatient(SelectedPatient.Id);
        Patients.Add(SelectedPatient);
        ArchivedPatients.Remove(SelectedPatient);
    }

    [RelayCommand]
    private async Task UpdatePatientAsync()
    {
        if (EditingPatient is null || SelectedPatient is null) return;

        try
        {
            _patientService.UpdatePatient(EditingPatient);

            EditingPatient.PhoneNo = FormatPhoneNumber(EditingPatient.PhoneNo);
            EditingPatient.EmergencyContact = FormatPhoneNumber(EditingPatient.EmergencyContact);

            LoadAllPatients();
            EditingPatient = null!;
            SelectedPatient = null!;

            await _dialogService.ShowAlertAsync("Patient updated successfully.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Update failed: {ex.Message}");
        }
    }

    [RelayCommand]
    public void SearchPatient()
    {
        var filter = new PatientFilter();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            if (SearchQuery.All(char.IsDigit) && SearchQuery.Length == 13)
            {
                filter.CNP = SearchQuery;
            }
            else
            {
                filter.NamePart = SearchQuery;
            }
        }

        List<Patient> results = _patientService.SearchPatients(filter);

        Patients.Clear();
        foreach (Patient p in results.Where(p => !p.IsArchived))
        {
            p.PhoneNo = FormatPhoneNumber(p.PhoneNo);
            p.EmergencyContact = FormatPhoneNumber(p.EmergencyContact);
            Patients.Add(p);
        }

        NoResultsFound = Patients.Count == 0 && !string.IsNullOrWhiteSpace(SearchQuery);
    }

    [RelayCommand]
    private async Task ExecuteFilterAsync()
    {
        try
        {
            Sex? finalSexEnum = null;

            if (SelectedSexFilter is Microsoft.UI.Xaml.Controls.ComboBoxItem item)
            {
                string? content = item.Content.ToString();
                if (Enum.TryParse(content, out Sex result))
                    finalSexEnum = result;
            }

            var filter = new PatientFilter
            {
                MinAge = (int?)MinAge,
                MaxAge = (int?)MaxAge,
                Sex = finalSexEnum,
            };

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                if (SearchQuery.All(char.IsDigit) && SearchQuery.Length == 13)
                    filter.CNP = SearchQuery;
                else
                    filter.NamePart = SearchQuery;
            }

            List<Patient> results = _patientService.SearchPatients(filter);

            Patients.Clear();
            foreach (Patient p in results.Where(x => !x.IsArchived))
            {
                p.PhoneNo = FormatPhoneNumber(p.PhoneNo);
                p.EmergencyContact = FormatPhoneNumber(p.EmergencyContact);
                Patients.Add(p);
            }

            NoResultsFound = Patients.Count == 0 && !string.IsNullOrWhiteSpace(SearchQuery);
        }
        catch (ArgumentException ex)
        {
            await _dialogService.ShowAlertAsync(ex.Message);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        MinAge = null;
        MaxAge = null;
        SelectedSexFilter = null!;
        SearchQuery = "";
        LoadAllPatients();
        NoResultsFound = false;
    }

    [RelayCommand]
    private async Task MarkAsDeceasedAsync()
    {
        if (SelectedPatient is null) return;

        DateTime? chosenDate = await (_dialogService.ShowDatePickerAsync("Enter Date of Death:", "Mark as Deceased") ?? Task.FromResult<DateTime?>(null));
        if (chosenDate is null)
        {
            return;
        }

        if (chosenDate > DateTime.Now)
        {
            await _dialogService.ShowAlertAsync("Date of death cannot be in the future.");
            return;
        }

        if (chosenDate < SelectedPatient.Dob)
        {
            await _dialogService.ShowAlertAsync("Date of death cannot be earlier than the Date of Birth.");
            return;
        }

        SelectedPatient.PhoneNo = SelectedPatient.PhoneNo
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal)
            .Replace("+40", "0", StringComparison.Ordinal);

        SelectedPatient.EmergencyContact = SelectedPatient.EmergencyContact
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal)
            .Replace("+40", "0", StringComparison.Ordinal);

        SelectedPatient.Dod = chosenDate;
        SelectedPatient.IsArchived = true;

        try
        {
            _patientService.UpdatePatient(SelectedPatient);
            LoadAllPatients();
            LoadArchivedPatients();
            OnPropertyChanged(nameof(IsNotDeceased));
            await _dialogService.ShowAlertAsync("This patient has now become a ghost. Beware!!!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenOrganDonorDialogAsync()
    {
        if (SelectedPatient?.IsDeceased != true || !SelectedPatient.IsDonor)
        {
            await _dialogService.ShowAlertAsync("Patient must be deceased and registered as a donor.");
            return;
        }

        await _dialogService.ShowOrganDonorDialogAsync(SelectedPatient);
    }

    [RelayCommand]
    private async Task MarkAsOrganDonorAsync()
    {
        if (SelectedPatient is null)
        {
            await _dialogService.ShowAlertAsync("Please select a patient first.");
            return;
        }

        if (!SelectedPatient.IsDeceased)
        {
            await _dialogService.ShowAlertAsync("Patient must be marked as deceased before registering as an organ donor.");
            return;
        }

        try
        {
            SelectedPatient.PhoneNo = SelectedPatient.PhoneNo
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("+40", "0", StringComparison.Ordinal);

            SelectedPatient.EmergencyContact = SelectedPatient.EmergencyContact
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("+40", "0", StringComparison.Ordinal);

            SelectedPatient.IsDonor = true;
            _patientService.UpdatePatient(SelectedPatient);
            await OpenOrganDonorDialogAsync();
            LoadAllPatients();
            LoadArchivedPatients();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error marking patient as organ donor: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ReportGhostAsync()
    {
        System.Diagnostics.Debug.WriteLine($">> GHOST REPORTED FROM ADMIN AT {DateTime.Now} <<");

        try
        {
            OnPropertyChanged(nameof(IsNotDeceased));
            await _dialogService.ShowAlertAsync("The patient has been marked as deceased. The record is now locked and moved to the archive.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error: {ex.Message}");
        }
    }

    #endregion Methods
}
