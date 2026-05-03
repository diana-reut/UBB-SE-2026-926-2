using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HospitalManagement.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace HospitalManagement.ViewModel;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    private object _currentView;
    // private int _ghostSightingCount = 0;
    // private DateTime _lastResetTime = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    // I SAW A GHOST LOGIC
    private readonly IGhostService _ghostService;

    private bool _isExorcismAlertVisible;

    public bool IsExorcismAlertVisible
    {
        get => _isExorcismAlertVisible;

        set
        {
            _isExorcismAlertVisible = value;
            OnPropertyChanged();
        }
    }

    public object CurrentView
    {
        get => _currentView;

        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public ICommand RedirectToAdminRoleCommand { get; }

    public ICommand RedirectToStaffRoleCommand { get; }

    public ICommand RedirectToPharmacistRoleCommand { get; }

    public ICommand GhostSightingCommand { get; }

    public MainWindowViewModel()
    {
        RedirectToAdminRoleCommand = new RelayCommand(RedirectToAdminRole);
        RedirectToStaffRoleCommand = new RelayCommand(RedirectToStaffRole);
        RedirectToPharmacistRoleCommand = new RelayCommand(RedirectToPharmacistRole);
        // GhostSightingCommand = new RelayCommand(RecordGhostSighting);

        _ghostService = ((App)Application.Current).Services.GetService<IGhostService>() ?? throw new InvalidOperationException("GhostService not registered in DI container.");
        _ghostService.ExorcismTriggered += (s, e) => IsExorcismAlertVisible = true;
        GhostSightingCommand = new RelayCommand(() => _ghostService.SawAGhost());
        IsExorcismAlertVisible = _ghostService.IsExorcismTriggered();
    }

    private void RedirectToAdminRole()
    {
        CurrentView = "AdminDashboard";
    }

    private void RedirectToStaffRole()
    {
        CurrentView = "StaffDashboard";
    }

    private void RedirectToPharmacistRole()
    {
        CurrentView = "PharmacistDashboard";
    }

    //// SV23
    // private void RecordGhostSighting()
    // {
    //    if ((DateTime.Now - _lastResetTime).TotalHours > 24)
    //    {
    //        _ghostSightingCount = 0;
    //        _lastResetTime = DateTime.Now;
    //    }

    // _ghostSightingCount++;

    // if (_ghostSightingCount > 3)
    //    {
    //        TriggerExorcismAlert();
    //    }
    // }

    // private void TriggerExorcismAlert()
    // {
    //   //this is an example
    //    string message = "CRITICAL PARANORMAL ACTIVITY DETECTED: Multiple sightings confirmed. Please CALL THE PRIEST immediately.";
    //    Console.WriteLine(message);
    // }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
