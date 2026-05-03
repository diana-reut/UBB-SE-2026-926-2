using HospitalManagement.Entity;
using HospitalManagement.Integration;
using HospitalManagement.Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HospitalManagement.ViewModel;
//m
internal class MedicalStaffViewModel : INotifyPropertyChanged
{
    private string _searchQuery = "";
    private string _errorMessage = "";
    private ObservableCollection<Patient> _searchResults = [];
    private readonly IPatientService _patientService;
    private Patient? _selectedPatient;

    private readonly IGhostService _ghostService;

    private bool _isExorcismAlertVisible;

    public Action<Patient>? OpenBloodDonorsAction { get; set; }
    public Action<Patient>? OpenTransplantRequestAction { get; set; }

    public bool IsExorcismAlertVisible
    {
        get => _isExorcismAlertVisible;

        set
        {
            _isExorcismAlertVisible = value;
            OnPropertyChanged();
        }
    }

    public Patient? SelectedPatient
    {
        get => _selectedPatient;

        set
        {
            _selectedPatient = value;
            OnPropertyChanged();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;

        set
        {
            _searchQuery = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;

        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Patient> SearchResults
    {
        get => _searchResults;

        set
        {
            _searchResults = value;
            OnPropertyChanged();
        }
    }

    public ICommand SearchCommand { get; set; }

    public ICommand? BackToMainCommand { get; set; }

    public ICommand GhostSightingCommand { get; set; }


    public ICommand FindBloodDonorsCommand { get; set; }

    public ICommand RequestTransplantCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MedicalStaffViewModel(IPatientService patientService, IGhostService ghostService)
    {
        _patientService =patientService;
        _ghostService =ghostService;
        _ghostService.ExorcismTriggered += (s, e) => IsExorcismAlertVisible = true;
        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();

        SearchCommand = new RelayCommand(ExecuteSearch);

        FindBloodDonorsCommand = new RelayCommand(FindBloodDonors);
        RequestTransplantCommand = new RelayCommand(RequestTransplant);
        GhostSightingCommand = new RelayCommand(() => _ghostService.SawAGhost());
    }

    private void ExecuteSearch()
    {
        ErrorMessage = "";
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        var filter = new PatientFilter();

        if (SearchQuery.Length == 13 && SearchQuery.All(char.IsDigit))
        {
            filter.CNP = SearchQuery;
        }
        else
        {
            filter.NamePart = SearchQuery;
        }

        try
        {
            System.Collections.Generic.List<Patient> results = _patientService.SearchPatients(filter);

            if (results is null || results.Count == 0)
            {
                ErrorMessage = "There are no patients with this name or CNP.";
            }
            else
            {
                foreach (Patient p in results)
                {
                    SearchResults.Add(p);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Database connection error: " + ex.Message;
        }
    }

    private void FindBloodDonors()
    {
        if (SelectedPatient is not null)
        {
            OpenBloodDonorsAction?.Invoke(SelectedPatient);
        }
    }

    private void RequestTransplant()
    {
        if (SelectedPatient is not null)
        {
            OpenTransplantRequestAction?.Invoke(SelectedPatient);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
