using Common.Data.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;

namespace HospitalManagement.View;

internal sealed partial class PatientProfileView : Page
{
    private readonly PatientProfileViewModel _viewModel;

    public PatientProfileView()
    {
        InitializeComponent();

        _viewModel = ServiceRegistry.Services.GetRequiredService<PatientProfileViewModel>();


        DataContext = _viewModel;

        _viewModel.ShowAlertAction = ShowAlertAsync;
        _viewModel.OpenFileAction = OpenFile;

        Loaded += Page_Loaded;
    }

    public async Task InitializeAsync(int patientId)
    {
        await _viewModel.LoadFullPatientProfileAsync(patientId);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.CheckHighRiskStatusAsync();
    }

    private async void ViewPrescription_ClickAsync(object sender, RoutedEventArgs e)
    {
        await _viewModel.ViewPrescriptionAsync();
    }

    private async void ExportPDF_ClickAsync(object sender, RoutedEventArgs e)
    {
        await _viewModel.ExportSelectedRecordAsync();
    }

    private async void ImportER_ClickAsync(object sender, RoutedEventArgs e)
    {
        await _viewModel.ImportRecordsAsync(isER: true);
    }

    private async void ImportStaff_ClickAsync(object sender, RoutedEventArgs e)
    {
        await _viewModel.ImportRecordsAsync(isER: false);
    }

    private void RecordList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is MedicalRecord clickedRecord)
        {
            _viewModel.SelectedRecord = clickedRecord;
        }
    }

    private void OnShowAlert(string title, string content)
    {
        ShowAlertAsync(title, content);
    }

    private void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ShowAlertAsync("Export Failed", "The exported PDF could not be found.");
            return;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
        };

        try
        {
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            ShowAlertAsync("Export Failed", $"Could not open the exported PDF. {ex.Message}");
        }
    }

    private async Task OnShowPrescriptionAsync(int prescriptionId)
    {
        var prescriptionWindow = new Window { Title = "Prescription Details" };
        PrescriptionView prescriptionPage = ServiceRegistry.Services.GetRequiredService<PrescriptionView>();

        prescriptionPage.ViewModel.SearchIdText = prescriptionId.ToString(CultureInfo.InvariantCulture);
        await prescriptionPage.ViewModel.ShowPrescriptionAsync(prescriptionId);

        prescriptionWindow.Content = prescriptionPage;
        prescriptionWindow.Activate();
    }

    private async void ShowAlertAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        _ = await dialog.ShowAsync();
    }
}
