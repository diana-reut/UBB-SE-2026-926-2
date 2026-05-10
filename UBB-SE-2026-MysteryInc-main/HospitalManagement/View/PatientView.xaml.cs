using Common.Data.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.View;

internal sealed partial class PatientView : Window
{
    private readonly PatientViewModel _viewModel;
    private Action _goBackCallback;

    public PatientView()
    {
        _goBackCallback = null!;
        InitializeComponent();

        _viewModel = ServiceRegistry.Services.GetRequiredService<PatientViewModel>();

        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = _viewModel;
        }

        MaximizeWindow();
        SetupViewModelActions();
    }

    public void Initialize(int patientId, Action goBackCallback)
    {
        _goBackCallback = goBackCallback;
        _viewModel.GoBackAction = GoBack;
        _ = _viewModel.LoadFullPatientProfileAsync(patientId);
    }

    private void SetupViewModelActions()
    {
        _viewModel.OpenRouletteAction = OpenRouletteAsync;
        _viewModel.OpenPrescriptionDialogAction = OpenPrescriptionDialogAsync;
        _viewModel.ShowAlertAction = ShowAlert;
    }


    private async void HandleRouletteResult(int discount, decimal finalPrice)
    {
        await _viewModel.HandleRouletteResultAsync(discount, finalPrice);
    }


    private async Task OpenRouletteAsync(decimal basePrice)
    {
        var rouletteDialog = new DiscountRouletteDialog
        {
            XamlRoot = GetDialogXamlRoot(),
        };
        rouletteDialog.ViewModel.Initialize(basePrice);
        rouletteDialog.ViewModel.SpinCompleted += HandleRouletteResult;
        _ = await rouletteDialog.ShowAsync();
        rouletteDialog.ViewModel.SpinCompleted -= HandleRouletteResult;
    }

    private async Task OpenPrescriptionDialogAsync(Prescription prescription)
    {
        var prescriptionDialogViewModel = new PrescriptionDialogViewModel();
        prescriptionDialogViewModel.Initialize(prescription);
        var prescriptionDialog = new PrescriptionDialog(prescriptionDialogViewModel)
        {
            XamlRoot = GetDialogXamlRoot(),
        };
        _ = await prescriptionDialog.ShowAsync();
    }

    private void MaximizeWindow()
    {
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }
    }

    private void GoBack()
    {
        _goBackCallback?.Invoke();
        Close();
    }

    private async void ShowAlert(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = GetDialogXamlRoot(),
        };

        _ = await dialog.ShowAsync();
    }

    private XamlRoot GetDialogXamlRoot()
    {
        if (Content is FrameworkElement rootElement && rootElement.XamlRoot is not null)
        {
            return rootElement.XamlRoot;
        }

        throw new InvalidOperationException("The current window is not attached to a XAML root.");
    }
}
