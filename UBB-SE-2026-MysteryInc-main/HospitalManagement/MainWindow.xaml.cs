using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using HospitalManagement.Infrastructure;
using HospitalManagement.View;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ServiceRegistry.SetMainWindow(this);

        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }

        ServiceRegistry.Configure(((App)Application.Current).Services);
        ERManagementSystem.Infrastructure.ServiceRegistry.Configure(((App)Application.Current).Services);
        ERManagementSystem.Infrastructure.ServiceRegistry.SetMainWindow(this);

        var navigationService = ((App)Application.Current).Services.GetRequiredService<NavigationService>();
        navigationService.Initialize(ContentFrame);

        NavigateToLogin();
    }

    private void NavigateToLogin()
    {
        LoginFrame.Navigate(typeof(LoginView));

        if (LoginFrame.Content is LoginView loginView)
        {
            loginView.ViewModel.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void OnLoginSucceeded(object? sender, System.EventArgs e)
    {
        if (sender is LoginViewModel vm)
        {
            vm.LoginSucceeded -= OnLoginSucceeded;
        }

        LoginFrame.Visibility = Visibility.Collapsed;
        AppNavigationView.IsEnabled = true;
        AppNavigationView.Visibility = Visibility.Visible;

        ContentFrame.Navigate(typeof(AdminDashboardPage));
        AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is not string tag)
        {
            return;
        }

        switch (tag)
        {
            case "AdminDashboard":
                ContentFrame.Navigate(typeof(AdminDashboardPage));
                break;
            case "MedicalStaffDashboard":
                ContentFrame.Navigate(typeof(MedicalStaffDashboardPage));
                break;
            case "PharmacistDashboard":
                ContentFrame.Navigate(typeof(PharmacistDashboardPage));
                break;
            case "PatientRegistration":
                ContentFrame.Navigate(typeof(PatientRegistrationView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<PatientRegistrationViewModel>());
                break;
            case "Queue":
                ContentFrame.Navigate(typeof(QueueView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<QueueViewModel>());
                break;
            case "Triage":
                ContentFrame.Navigate(typeof(TriageView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<TriageViewModel>());
                break;
            case "RoomAssignment":
                ContentFrame.Navigate(typeof(RoomAssignmentView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<RoomAssignmentViewModel>());
                break;
            case "Examination":
                ContentFrame.Navigate(typeof(ExaminationView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<ExaminationViewModel>());
                break;
            case "TransferLog":
                ContentFrame.Navigate(typeof(TransferLogView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<TransferLogViewModel>());
                break;
            case "RoomManagement":
                ContentFrame.Navigate(typeof(RoomManagementView), ERManagementSystem.Infrastructure.ServiceRegistry.Services.GetRequiredService<RoomManagementViewModel>());
                break;
        }
    }
}
