using Common.Data.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Common.Data.Entity.Enums;
using HospitalManagement.Infrastructure;
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
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Proxy.TransplantProxy;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.ViewModel;

internal partial class AdminViewModel : ObservableObject
{
    #region Variables

    private readonly IPatientProxy _patientService;
    private readonly IGhostService _ghostService;
    private readonly ITransplantProxy _transplantProxy;
    private readonly IDialogService _dialogService;
    private PatientView? _patientDetailsWindow;
    private bool _isOpeningPatientDetails;

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

    partial void OnIsStatisticsVisibleChanged(bool value) =>
        OnPropertyChanged(nameof(IsPatientPanelVisible));

    [RelayCommand]
    private static void NavigateHome()
    {
        Window mainWindow = ServiceRegistry.MainWindow;
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

    public Patient? NewPatient { get; set; }

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
            OnPropertyChanged(nameof(IsPatientPanelVisible));

            if (_selectedPatient is not null)
            {
                EditingPatient = _selectedPatient;
            }
        }
    }

    #endregion Patients

    #region State

    public bool IsActiveMode => !IsArchivedMode;

    public bool IsNotDeceased => SelectedPatient?.IsDeceased == false;

    public bool IsDeceased => SelectedPatient?.IsDeceased == true;

    public bool IsPatientPanelVisible => SelectedPatient is not null && !IsStatisticsVisible;

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
            _ = SearchPatientAsync();
        }
    }

    #endregion Filters

    #region Constructor

    public AdminViewModel()
    {
        _ghostService = ServiceRegistry.Services.GetRequiredService<IGhostService>();
        _patientService = ServiceRegistry.Services.GetRequiredService<IPatientProxy>();
        _transplantProxy = ServiceRegistry.Services.GetRequiredService<ITransplantProxy>();
        _dialogService = ServiceRegistry.Services.GetRequiredService<IDialogService>();

        Patients = [];
        ArchivedPatients = [];
        _ghostService.ExorcismTriggered += (s, e) => IsExorcismAlertVisible = true;
        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();

        _ = LoadAllPatientsAsync();
        CurrentView = "AdminDashboard";
    }

    #endregion Constructor

    #region Methods

    [RelayCommand]
    public void GhostSighting() => _ghostService.SawAGhost();


    [RelayCommand]
    public async Task LoadAllPatientsAsync()
    {
        var emptyFilter = new SearchPatientsDto();
        List<Patient> allPatients = await _patientService.SearchPatientsAsync(emptyFilter);

        Patients.Clear();
        foreach (Patient patient in allPatients.Where(p => !p.IsArchived))
        {
            patient.PhoneNo = FormatPhoneNumber(patient.PhoneNo);
            patient.EmergencyContact = FormatPhoneNumber(patient.EmergencyContact);
            Patients.Add(patient);
        }
    }


    [RelayCommand]
    public async Task LoadArchivedPatientsAsync()
    {
        IsArchivedMode = true;
        var emptyFilter = new SearchPatientsDto();
        List<Patient> allPatients = await _patientService.SearchPatientsAsync(emptyFilter);

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
        if (SelectedPatient is null || _isOpeningPatientDetails)
        {
            return;
        }

        if (_patientDetailsWindow is not null)
        {
            _patientDetailsWindow.Initialize(SelectedPatient.Id, () => { });
            _patientDetailsWindow.Activate();
            return;
        }

        try
        {
            _isOpeningPatientDetails = true;

            IServiceProvider scope = ServiceRegistry.Services;
            PatientView patientWindow = scope.GetRequiredService<PatientView>();
            patientWindow.Closed += (_, _) => _patientDetailsWindow = null;
            patientWindow.Initialize(SelectedPatient.Id, () => { });

            _patientDetailsWindow = patientWindow;
            patientWindow.Activate();
        }
        finally
        {
            _isOpeningPatientDetails = false;
        }
    }

    public async Task AssignOrganDonorAsync(int transplantId, int donorId, float score, string donorName)
    {
        try
        {
            await _transplantProxy.AssignDonorAsync(transplantId, donorId, score);
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
            var dto = new CreateMedicalHistoryDto
            {
                BloodType = history.BloodType,
                Rh = history.Rh,
                ChronicConditions = history.ChronicConditions,
                AllergyIds = history.PatientAllergies
                    .ConvertAll(pa => pa.AllergyId)
,
            };

            await _patientService.CreateMedicalHistoryAsync(patientId, dto);
            await _dialogService.ShowAlertAsync("Medical history saved successfully!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error saving medical history: {ex.Message}");
        }
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        phone = phone.Replace(" ", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal);

        if (!phone.StartsWith('0') || phone.Length != 10)
        {
            return phone;
        }

        return $"+40 {phone.Substring(1, 3)} {phone.Substring(4, 3)} {phone.Substring(7, 3)}";
    }

    [RelayCommand]
    private async Task AddPatientAsync()
    {
        Patient? patient = await _dialogService.ShowAddPatientDialogAsync();
        if (patient is null)
            return;

        try
        {
            patient.PhoneNo = FormatPhoneNumber(patient.PhoneNo);
            patient.EmergencyContact = FormatPhoneNumber(patient.EmergencyContact);
            Patients.Add(patient);

            MedicalHistoryEntry entry = await _dialogService.ShowMedicalHistoryAsync();
            // patient.Id is now set because CreatePatient populated it via EF
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
        if (SelectedPatient is null)
            return;

        bool isConfirmed = await (_dialogService.ShowConfirmAsync(
            $"Are you sure you want to archive {SelectedPatient.FirstName} {SelectedPatient.LastName}?",
            "Confirm Archive")
            ?? Task.FromResult(false));

        if (!isConfirmed) return;

        await _patientService.ArchivePatientAsync(SelectedPatient.Id);
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

        await _patientService.DearchivePatientAsync(SelectedPatient.Id);
        Patients.Add(SelectedPatient);
        ArchivedPatients.Remove(SelectedPatient);
    }

    [RelayCommand]
    private async Task UpdatePatientAsync()
    {
        if (EditingPatient is null || SelectedPatient is null) return;

        try
        {
            var dto = new UpdatePatientDto
            {
                FirstName = EditingPatient.FirstName,
                LastName = EditingPatient.LastName,
                Cnp = EditingPatient.Cnp,
                Dob = EditingPatient.Dob,
                Sex = EditingPatient.Sex,
                PhoneNo = EditingPatient.PhoneNo,
                EmergencyContact = EditingPatient.EmergencyContact,
                IsDonor = EditingPatient.IsDonor,
                IsArchived = EditingPatient.IsArchived,
            };

            await _patientService.UpdatePatientAsync(EditingPatient.Id, dto);

            EditingPatient.PhoneNo = FormatPhoneNumber(EditingPatient.PhoneNo);
            EditingPatient.EmergencyContact = FormatPhoneNumber(EditingPatient.EmergencyContact);

            await LoadAllPatientsAsync();
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
    public async Task SearchPatientAsync()
    {
        var filter = new SearchPatientsDto();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            if (SearchQuery.All(char.IsDigit) && SearchQuery.Length == 13)
            {
                filter.Cnp = SearchQuery;
            }
            else
            {
                filter.NamePart = SearchQuery;
            }
        }

        List<Patient> results = await _patientService.SearchPatientsAsync(filter);

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

            var filter = new SearchPatientsDto
            {
                MinAge = (int?)MinAge,
                MaxAge = (int?)MaxAge,
                Sex = finalSexEnum,
            };

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                if (SearchQuery.All(char.IsDigit) && SearchQuery.Length == 13)
                    filter.Cnp = SearchQuery;
                else
                    filter.NamePart = SearchQuery;
            }

            List<Patient> results = await _patientService.SearchPatientsAsync(filter);

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
    private async Task ClearFilters()
    {
        MinAge = null;
        MaxAge = null;
        SelectedSexFilter = null!;
        SearchQuery = "";
        await LoadAllPatientsAsync();
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

        try
        {
            await _patientService.ArchiveAsDeceasedAsync(SelectedPatient.Id, new ArchiveAsDeceasedDto
            {
                DeathDate = chosenDate.Value,
            });
            await LoadAllPatientsAsync();
            await LoadArchivedPatientsAsync();
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
            var dto = new UpdatePatientDto
            {
                FirstName = SelectedPatient.FirstName,
                LastName = SelectedPatient.LastName,
                Cnp = SelectedPatient.Cnp,
                Dob = SelectedPatient.Dob,
                Dod = SelectedPatient.Dod,
                Sex = SelectedPatient.Sex,
                PhoneNo = SelectedPatient.PhoneNo
                    .Replace(" ", "", StringComparison.Ordinal)
                    .Replace("+40", "0", StringComparison.Ordinal),
                EmergencyContact = SelectedPatient.EmergencyContact
                    .Replace(" ", "", StringComparison.Ordinal)
                    .Replace("+40", "0", StringComparison.Ordinal),
                IsDonor = true,
                IsArchived = SelectedPatient.IsArchived,
                Transferred = SelectedPatient.Transferred,
            };

            await _patientService.UpdatePatientAsync(SelectedPatient.Id, dto);
            SelectedPatient.IsDonor = true;
            await OpenOrganDonorDialogAsync();
            await LoadAllPatientsAsync();
            await LoadArchivedPatientsAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error marking patient as organ donor: {ex.Message}");
        }
    }

    #endregion Methods
}
