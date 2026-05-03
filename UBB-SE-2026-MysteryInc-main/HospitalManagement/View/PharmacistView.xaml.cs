using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;

namespace HospitalManagement.View;

public sealed partial class PharmacistView : Window
{
    public PharmacistViewModel ViewModel { get; }

    public PharmacistView()
    {
        ViewModel = (Application.Current as App)!.Services.GetRequiredService<PharmacistViewModel>();

        InitializeComponent();

        if (this.Content is FrameworkElement root)
        {
            root.DataContext = ViewModel;
        }

        MaximizeWindow();

        ViewModel.RequestClose += () => this.Close();
    }

    private void MaximizeWindow()
    {
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }
    }

    private void BackToHomeButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateBackToHome();
    }
}
