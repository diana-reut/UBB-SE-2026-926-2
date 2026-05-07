using HospitalManagement.Entity;
using HospitalManagement.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal class BloodDonorsViewModel : INotifyPropertyChanged
{
    private readonly IBloodCompatibilityService _bloodService;
    private readonly IPatientService _patientService;
    private string _statusMessage = string.Empty;

    public ObservableCollection<DonorMatchModel> Donors { get; } = [];

    public string StatusMessage
    {
        get => _statusMessage;

        set
        {
            if (string.Equals(_statusMessage, value, StringComparison.Ordinal))
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BloodDonorsViewModel(IBloodCompatibilityService bloodService, IPatientService patientService)
    {
        _bloodService = bloodService ?? throw new ArgumentNullException(nameof(bloodService));
        _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
    }

    public void LoadCompatibleDonors(int patientId)
    {
        LoadCompatibleDonorsAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task LoadCompatibleDonorsAsync(int patientId)
    {
        StatusMessage = string.Empty;
        Donors.Clear();

        Patient? recipient = await _patientService.GetPatientDetailsAsync(patientId);
        if (recipient?.MedicalHistory is null
            || recipient.MedicalHistory.BloodType is null
            || recipient.MedicalHistory.Rh is null)
        {
            StatusMessage = "The selected patient needs a blood type and Rh factor in their medical history first.";
            return;
        }

        List<Patient> topDonors = await _bloodService.GetTopCompatibleDonorsAsync(patientId);
        foreach (Patient donor in topDonors)
        {
            int matchScore = _bloodService.CalculateScore(donor, recipient);

            Donors.Add(new DonorMatchModel
            {
                FirstName = donor.FirstName,
                LastName = donor.LastName,
                Cnp = donor.Cnp,
                BloodType = donor.MedicalHistory?.BloodType?.ToString() ?? "Unknown",
                RhFactor = donor.MedicalHistory?.Rh?.ToString() ?? "Unknown",
                Score = matchScore,
            });
        }

        if (Donors.Count == 0)
        {
            StatusMessage = "No compatible blood donors were found for this patient.";
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

