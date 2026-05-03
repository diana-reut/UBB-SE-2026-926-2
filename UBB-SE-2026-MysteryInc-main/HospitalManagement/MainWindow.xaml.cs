using HospitalManagement.View;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace HospitalManagement;

public sealed partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        // FULL SCREEN
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }

        _viewModel = new MainWindowViewModel();
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = _viewModel;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentView))
        {
            if (_viewModel.CurrentView?.ToString() == "PharmacistDashboard")
            {
                PharmacistView pharmacistWindow = (App.Current as App).Services.GetRequiredService<PharmacistView>();
                pharmacistWindow.Activate();
                Close();
            }
            // AICI VA CONECTATI VOI CU IF ELSE
            else if (_viewModel.CurrentView?.ToString() == "AdminDashboard")
            {
                AdminView adminWindow = (App.Current as App)!.Services.GetRequiredService<View.AdminView>();
                adminWindow.Activate();
                Close();
            }
            else if (_viewModel.CurrentView?.ToString() == "StaffDashboard")
            {
                // 1. Create your new Medical Staff Window
                var staffWindow = new View.MedicalStaffView();

                // 2. Setup the "Back to Main" button logic
                // We grab your ViewModel and tell it what the BackToMainCommand should actually do
                if (staffWindow.Content is FrameworkElement root && root.DataContext is MedicalStaffViewModel staffVM)
                {
                    // We use RelayCommand (from your MainViewModel file) to handle the click
                    staffVM.BackToMainCommand = new RelayCommand(() =>
                    {
                        // Open a new Main Window and close the Staff Window
                        var roleSelection = new MainWindow();
                        roleSelection.Activate();
                        staffWindow.Close();
                    });

                    // Force the UI to refresh its binding to pick up the new command
                    root.DataContext = null;
                    root.DataContext = staffVM;
                }

                // 3. Show your new window and close the main login window
                staffWindow.Activate();
                Close();
            }
        }
    }
}
