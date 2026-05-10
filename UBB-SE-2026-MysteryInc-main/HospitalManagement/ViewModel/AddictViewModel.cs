using Common.Data.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Proxy.AddictDetectionProxy;
using HospitalManagement.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace HospitalManagement.ViewModel;

internal partial class AddictViewModel : ObservableObject
{
    private readonly IAddictDetectionProxy _addictDetectionProxy;

    [ObservableProperty]
    private ObservableCollection<Patient> _addictCandidates = [];

    public AddictViewModel(IAddictDetectionProxy addictDetectionService)
    {
        _addictDetectionProxy = addictDetectionService ?? throw new ArgumentNullException(nameof(addictDetectionService));
        _ = LoadAddictsAsync();
    }

    public void LoadAddicts()
    {
        LoadAddictsAsync().GetAwaiter().GetResult();
    }

    public async Task LoadAddictsAsync()
    {
        List<Patient> candidates = await _addictDetectionProxy.GetAddictCandidatesAsync();
        AddictCandidates = new ObservableCollection<Patient>(candidates);
    }

    public string GetPoliceReportMessage(int patientId)
    {
        return GetPoliceReportMessageAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task<string> GetPoliceReportMessageAsync(int patientId)
    {
        Patient? targetPatient = AddictCandidates.FirstOrDefault(p => p.Id == patientId);
        if (targetPatient is null)
        {
            return "Error: Patient not found in the current flagged list.";
        }

        return await _addictDetectionProxy.BuildPoliceReportAsync(targetPatient);
    }

    public void RemoveFlaggedPatient(int patientId)
    {
        Patient? targetPatient = AddictCandidates.FirstOrDefault(p => p.Id == patientId);
        if (targetPatient is not null)
        {
            _ = AddictCandidates.Remove(targetPatient);
        }
    }

    private static void PlayPoliceAlert()
    {
        _ = Task.Run(() =>
        {
            Console.Beep(1200, 200);
            Console.Beep(800, 200);
            Console.Beep(1200, 200);
            Console.Beep(800, 200);
            Console.Beep(1500, 500);
        });
    }

    [RelayCommand]
    public void ConfirmPoliceAlert(int patientId)
    {
        PlayPoliceAlert();
        RemoveFlaggedPatient(patientId);
    }
}
