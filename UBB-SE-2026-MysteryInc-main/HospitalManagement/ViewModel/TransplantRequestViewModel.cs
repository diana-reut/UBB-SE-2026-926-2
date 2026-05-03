using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Service;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal partial class TransplantRequestViewModel : ObservableObject
{
    private readonly ITransplantService _transplantService;
    private readonly IPatientService _patientService;

    private int _patientId;

    public Action? CloseWindowAction { get; set; }


    public Func<string, string, Task>? ShowDialogAction { get; set; }

    public TransplantRequestViewModel(
        ITransplantService transplantService,
        IPatientService patientService)
    {
        _transplantService = transplantService;
        _patientService = patientService;
    }

    [ObservableProperty]
    private string patientName = string.Empty;

    [ObservableProperty]
    private bool isUrgent;

    [ObservableProperty]
    private string? warningMessage;

    public Visibility UrgentVisibility =>
        IsUrgent ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WarningVisibility =>
        string.IsNullOrEmpty(WarningMessage)
            ? Visibility.Collapsed
            : Visibility.Visible;

    partial void OnIsUrgentChanged(bool value)
        => OnPropertyChanged(nameof(UrgentVisibility));

    partial void OnWarningMessageChanged(string? value)
        => OnPropertyChanged(nameof(WarningVisibility));

    [ObservableProperty]
    private string? selectedOrgan;

    [ObservableProperty]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnErrorMessageChanged(string? value)
        => OnPropertyChanged(nameof(HasError));

    [ObservableProperty]
    private bool requestSucceeded;


    public void Initialize(int patientId)
    {
        _patientId = patientId;

        var patient = _patientService.GetById(patientId);

        if (patient is not null)
            PatientName = $"{patient.FirstName} {patient.LastName}";

        IsUrgent = _transplantService.IsUrgent(patientId);
        WarningMessage = _transplantService.GetChronicWarning(patientId);
    }

    [RelayCommand]
    private async Task SubmitRequest()
    {
        ErrorMessage = null;
        RequestSucceeded = false;

        if (string.IsNullOrEmpty(SelectedOrgan))
        {
            ErrorMessage = "Please select an organ type.";
            return;
        }

        try
        {
            _transplantService.CreateWaitlistRequest(_patientId, SelectedOrgan);

            RequestSucceeded = true;

            if (ShowDialogAction is not null)
            {
                await ShowDialogAction(
                    "Success",
                    "The patient has been successfully added to the Organ Transplant Waitlist.");
            }

            CloseWindowAction?.Invoke();
        }
        catch (MyNotImplementedException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseWindowAction?.Invoke();
    }
}
