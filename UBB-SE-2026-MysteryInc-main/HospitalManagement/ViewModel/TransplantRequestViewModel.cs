using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Proxy.TransplantProxy;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal partial class TransplantRequestViewModel : ObservableObject
{
    private readonly ITransplantProxy _transplantProxy;
    private readonly IPatientProxy _patientService;

    private int _patientId;

    public Action? CloseWindowAction { get; set; }


    public Func<string, string, Task>? ShowDialogAction { get; set; }

    public TransplantRequestViewModel(
        ITransplantProxy transplantProxy,
        IPatientProxy patientService)
    {
        _transplantProxy = transplantProxy;
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
        InitializeAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task InitializeAsync(int patientId)
    {
        _patientId = patientId;

        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is not null)
            PatientName = $"{patient.FirstName} {patient.LastName}";

        IsUrgent = await _transplantProxy.IsUrgentAsync(patientId);
        WarningMessage = await _transplantProxy.GetChronicWarningAsync(patientId);
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
            await _transplantProxy.CreateWaitlistRequestAsync(_patientId, SelectedOrgan);

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
