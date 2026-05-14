using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Service;
using HospitalManagement.View;
using HospitalManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HospitalManagement.ViewModel;

public partial class PharmacistViewModel : ObservableObject
{
    private readonly IGhostService _ghostService;
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private object? _currentContent;

    [ObservableProperty]
    private bool _isExorcismAlertVisible;

    public event Action? RequestClose;

    public PharmacistViewModel()
    {
        _ghostService = ServiceRegistry.Services.GetRequiredService<IGhostService>();
        _services = ServiceRegistry.Services;

        _ghostService.ExorcismTriggered += OnExorcismTriggered;

        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();

        ShowPrescriptions();
    }

    [RelayCommand]
    private void ShowPrescriptions()
    {
        try
        {
            CurrentContent = _services.GetRequiredService<PrescriptionView>();
        }
        catch( Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ShowAddicts()
    {
        try
        {
            CurrentContent = _services.GetRequiredService<AddictView>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ReportGhost()
    {
        try
        {
            _ghostService.SawAGhost();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation Error: {ex.Message}");
        }
    }

    public void NavigateBackToHome()
    {
        Window mainWindow = ServiceRegistry.MainWindow;
        mainWindow.Activate();
        RequestClose?.Invoke();
    }

    private void OnExorcismTriggered(object? sender, EventArgs e)
    {
        IsExorcismAlertVisible = true;
    }
}
