using Common.Data.Entity;
using HospitalManagement.Proxy.PatientProxy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HospitalManagement.Proxy.BloodCompatibilityProxy;

namespace HospitalManagement.ViewModel;

internal class BloodDonorsViewModel : INotifyPropertyChanged
{
    private readonly IBloodCompatibilityProxy _bloodProxy;
    private readonly IPatientProxy _patientProxy;
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

    public BloodDonorsViewModel(IBloodCompatibilityProxy bloodProxy, IPatientProxy patientService)
    {
        _bloodProxy = bloodProxy ?? throw new ArgumentNullException(nameof(bloodProxy));
        _patientProxy = patientService ?? throw new ArgumentNullException(nameof(patientService));
    }

    public void LoadCompatibleDonors(int patientId)
    {
        LoadCompatibleDonorsAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task LoadCompatibleDonorsAsync(int patientId)
    {
        StatusMessage = string.Empty;
        Donors.Clear();

        Patient? recipient = await _patientProxy.GetPatientDetailsAsync(patientId);
        if (recipient?.MedicalHistory is null
            || recipient.MedicalHistory.BloodType is null
            || recipient.MedicalHistory.Rh is null)
        {
            StatusMessage = "The selected patient needs a blood type and Rh factor in their medical history first.";
            return;
        }
        List<Patient> topDonors = await _bloodProxy.GetTopCompatibleDonorsAsync(patientId);

        foreach (Patient donor in topDonors)
        {
            int matchScore = CalculateScore(donor, recipient);

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

    private int CalculateScore(Patient donor, Patient recipient)
    {
        int total = 0;

        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
            return 0;

        total += donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType &&
                 donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? 50
            : 25;

        int ageGap = Math.Abs(donor.Dob.Year - recipient.Dob.Year);
        total += Math.Max(0, 30 - ageGap / 5 * 5);

        total += donor.Sex == recipient.Sex ? 20 : 10;

        return total;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

