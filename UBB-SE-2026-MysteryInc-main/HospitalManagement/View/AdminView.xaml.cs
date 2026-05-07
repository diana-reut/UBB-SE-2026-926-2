using HospitalManagement.Infrastructure;
using HospitalManagement.View.DialogServiceAdmin;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace HospitalManagement.View;

internal sealed partial class AdminView : Window
{
    private readonly AdminViewModel _viewModel;
    private readonly StatisticsView _statisticsControl;

    private void SetupWindow()
    {
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }
    }

    public AdminView()
    {
        InitializeComponent();
        SetupWindow();

        IDialogService dialogService = ServiceRegistry.Services.GetRequiredService<IDialogService>();
        dialogService.SetWindow(this);

        _statisticsControl = ServiceRegistry.Services.GetRequiredService<StatisticsView>();
        StatisticsContainer.Child = _statisticsControl;

        _viewModel = ServiceRegistry.Services.GetRequiredService<AdminViewModel>();

        RootGrid.DataContext = _viewModel;
    }

    private void PatientListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        _viewModel?.OpenPatientDetailsCommand.Execute(null);
    }
}
