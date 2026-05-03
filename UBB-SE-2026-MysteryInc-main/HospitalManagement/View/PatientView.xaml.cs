using HospitalManagement.Entity;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
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

        _viewModel = (Application.Current as App)!.Services.GetRequiredService<PatientViewModel>();

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
        _viewModel.LoadFullPatientProfile(patientId);
    }

    private void SetupViewModelActions()
    {
        _viewModel.OpenRouletteAction = OpenRouletteAsync;
        _viewModel.OpenPrescriptionDialogAction = OpenPrescriptionDialogAsync;
    }

    private async Task OpenRouletteAsync(decimal basePrice)
    {
        var rouletteDialog = new DiscountRouletteDialog
        {
            XamlRoot = Content.XamlRoot,
        };
        rouletteDialog.ViewModel.Initialize(basePrice);
        rouletteDialog.ViewModel.SpinCompleted += _viewModel.HandleRouletteResult;
        _ = await rouletteDialog.ShowAsync();
        rouletteDialog.ViewModel.SpinCompleted -= _viewModel.HandleRouletteResult;
    }

    private async Task OpenPrescriptionDialogAsync(Prescription prescription)
    {
        var prescriptionDialogViewModel = new PrescriptionDialogViewModel();
        prescriptionDialogViewModel.Initialize(prescription);
        var prescriptionDialog = new PrescriptionDialog(prescriptionDialogViewModel);
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
}
