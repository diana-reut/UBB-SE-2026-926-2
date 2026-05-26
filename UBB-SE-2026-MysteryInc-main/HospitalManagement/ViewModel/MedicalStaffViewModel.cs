using CommunityToolkit.Mvvm.Input;
using Common.Data.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.Integration;
using HospitalManagement.Service;
using HospitalManagement.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.Data.Integration;
using HospitalManagement.Proxy.PatientProxy;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.ViewModel;

internal partial class MedicalStaffViewModel : INotifyPropertyChanged
{
    private string _searchQuery = "";
    private string _errorMessage = "";
    private ObservableCollection<Patient> _searchResults = [];
    private readonly IPatientProxy _patientService;
    private Patient? _selectedPatient;
    private readonly IGhostService _ghostService;
    private bool _isExorcismAlertVisible;

    public bool IsExorcismAlertVisible
    {
        get => _isExorcismAlertVisible;

        set
        {
            if (_isExorcismAlertVisible == value)
            {
                return;
            }

            _isExorcismAlertVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExorcismAlertVisibility));
        }
    }

    public Visibility ExorcismAlertVisibility =>
        IsExorcismAlertVisible ? Visibility.Visible : Visibility.Collapsed;

    public Patient? SelectedPatient
    {
        get => _selectedPatient;

        set
        {
            if (ReferenceEquals(_selectedPatient, value))
            {
                return;
            }

            _selectedPatient = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedPatientVisibility));
        }
    }

    public Visibility SelectedPatientVisibility =>
        SelectedPatient is null ? Visibility.Collapsed : Visibility.Visible;

    public string SearchQuery
    {
        get => _searchQuery;

        set
        {
            if (string.Equals(_searchQuery, value, StringComparison.Ordinal))
            {
                return;
            }

            _searchQuery = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;

        set
        {
            if (string.Equals(_errorMessage, value, StringComparison.Ordinal))
            {
                return;
            }

            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Patient> SearchResults
    {
        get => _searchResults;

        set
        {
            if (ReferenceEquals(_searchResults, value))
            {
                return;
            }

            _searchResults = value;
            OnPropertyChanged();
        }
    }

    public ICommand SearchCommand { get; set; }

    public ICommand? BackToMainCommand { get; set; }

    public ICommand GhostSightingCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MedicalStaffViewModel(IPatientProxy patientService, IGhostService ghostService)
    {
        _patientService = patientService;
        _ghostService = ghostService;
        _ghostService.ExorcismTriggered += (s, e) => IsExorcismAlertVisible = true;
        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();

        SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync);
        GhostSightingCommand = new RelayCommand(() => _ghostService.SawAGhost());
    }

    private async Task ExecuteSearchAsync()
    {
        ErrorMessage = "";
        SearchResults.Clear();

        var dto = new SearchPatientsDto();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            string trimmedQuery = SearchQuery.Trim();
            if (trimmedQuery.Length == 13 && trimmedQuery.All(char.IsDigit))
            {
                dto.Cnp = trimmedQuery;
            }
            else
            {
                dto.NamePart = trimmedQuery;
            }
        }

        try
        {
            System.Collections.Generic.List<Patient> results = await _patientService.SearchPatientsAsync(dto);

            if (results is null || results.Count == 0)
            {
                ErrorMessage = string.IsNullOrWhiteSpace(SearchQuery)
                    ? "There are no patients."
                    : "There are no patients with this name or CNP.";
            }
            else
            {
                foreach (Patient patient in results)
                {
                    SearchResults.Add(patient);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Database connection error: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task FindBloodDonorsAsync()
    {
        if (SelectedPatient is null)
        {
            ErrorMessage = "Please select a patient first.";
            return;
        }

        ErrorMessage = "";

        try
        {
            var donorsWindow = new Window
            {
                Title = $"Compatible Donors - {SelectedPatient.FirstName} {SelectedPatient.LastName}",
            };

            IServiceProvider services = ServiceRegistry.Services;
            BloodDonorsView donorsPage = services.GetRequiredService<BloodDonorsView>();
            donorsWindow.Content = donorsPage;
            donorsWindow.Activate();
            await donorsPage.InitializeAsync(SelectedPatient.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load compatible donors: " + ex.Message;
        }
    }

    [RelayCommand]
    public async Task RequestTransplantAsync()
    {
        if (SelectedPatient is null)
        {
            ErrorMessage = "Please select a patient first.";
            return;
        }

        ErrorMessage = "";

        try
        {
            var requestWindow = new Window
            {
                Title = $"Organ Transplant Request - {SelectedPatient.FirstName} {SelectedPatient.LastName}",
            };

            var requestPage = new TransplantRequestView(SelectedPatient.Id, requestWindow);
            await requestPage.InitializeAsync();

            requestWindow.Content = requestPage;
            requestWindow.Activate();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not open transplant request: " + ex.Message;
        }
    }


    public async Task OpenPatientProfileAsync(Patient? patient = null)
    {
        Patient? selectedPatient = patient ?? SelectedPatient;
        if (selectedPatient is null)
        {
            return;
        }

        var profileWindow = new Window
        {
            Title = "Patient Medical Profile",
        };

        IServiceProvider services = ServiceRegistry.Services;
        PatientProfileView profilePage = services.GetRequiredService<PatientProfileView>();
        await profilePage.InitializeAsync(selectedPatient.Id);

        profileWindow.Content = profilePage;
        profileWindow.Activate();
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
