using HospitalManagement.Entity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HospitalManagement.ViewModel;
using HospitalManagement.Service;

namespace HospitalManagement.View.DialogServiceAdmin;

internal class DialogService : IDialogService
{
    private Window? _window;
    
    public void SetWindow(Window window)
    {
        _window = window;
    }

    private XamlRoot XamlRoot
        => (_window?.Content as FrameworkElement)?.XamlRoot
            ?? throw new InvalidOperationException("Window not set");

    public async Task ShowAlertAsync(string message, string title = "System Message")
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };

        _ = await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmAsync(string message, string title)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    public async Task<DateTime?> ShowDatePickerAsync(string message, string title)
    {
        var picker = new DatePicker { Header = message, };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = picker,
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot,
        };

        ContentDialogResult result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary
            ? picker.Date.DateTime
            : null;
    }

    public async Task<MedicalHistoryEntry> ShowMedicalHistoryAsync()
    {
        var viewModel = (Application.Current as App)!.Services
        .GetRequiredService<MedicalHistoryDialogViewModel>();

        var dialog = new MedicalHistoryDialog(viewModel)
        {
            XamlRoot = XamlRoot,
        };

        dialog.Initialize();

        _ = await dialog.ShowAsync();

        return new MedicalHistoryEntry
        {
            History = dialog.MedicalHistory,
            WasSkipped = dialog.WasSkipped,
        };
    }

    public async Task ShowOrganDonorDialogAsync(Patient patient)
    {
        if (patient is null)
        {
            return;
        }

        OrganDonorDialog dialog = (Application.Current as App)!.Services
            .GetRequiredService<OrganDonorDialog>();

        dialog.XamlRoot = XamlRoot;

        dialog.Initialize(
            patient,
            async (transplantId, donorId, score) =>
            {
            });

        _ = await dialog.ShowAsync();
    }

    public async Task<Patient?> ShowAddPatientDialogAsync()
    {
        var dialog = new AddPatientDialog
        {
            XamlRoot = XamlRoot,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        return result == ContentDialogResult.Primary
            ? dialog.NewPatient
            : null;
    }
}
