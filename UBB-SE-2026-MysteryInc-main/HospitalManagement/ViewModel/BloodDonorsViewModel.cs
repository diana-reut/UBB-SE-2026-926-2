using HospitalManagement.Entity;
using HospitalManagement.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HospitalManagement.ViewModel;

internal class BloodDonorsViewModel : INotifyPropertyChanged
{
    private readonly IBloodCompatibilityService _bloodService;
    private readonly IPatientService _patientService;
    private ObservableCollection<DonorMatchModel>? _donors;

    public ObservableCollection<DonorMatchModel>? Donors
    {
        get => _donors;

        set
        {
            _donors = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BloodDonorsViewModel(IBloodCompatibilityService bloodService, IPatientService patientService)
    {
        _bloodService = bloodService ?? throw new ArgumentNullException(nameof(bloodService));
        _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        Donors = [];
    }

    public void LoadCompatibleDonors(int patientId)
    {
        Patient? recipient = _patientService.GetPatientDetails(patientId);
        List<Patient> topDonors = _bloodService.GetTopCompatibleDonors(patientId);

        var displayList = new ObservableCollection<DonorMatchModel>();
        if (recipient?.MedicalHistory is not null && topDonors is not null)
        {
            foreach (Patient donor in topDonors)
            {
                int matchScore = _bloodService.CalculateScore(donor, recipient);

                displayList.Add(new DonorMatchModel
                {
                    FirstName = donor.FirstName,
                    LastName = donor.LastName,
                    Cnp = donor.Cnp,
                    BloodType = donor.MedicalHistory?.BloodType?.ToString() ?? "Unknown",
                    RhFactor = donor.MedicalHistory?.Rh?.ToString() ?? "Unknown",
                    Score = matchScore,
                });
            }
        }

        Donors = displayList;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
