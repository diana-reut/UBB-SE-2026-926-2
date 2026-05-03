using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IBloodCompatibilityService
{
    public int CalculateScore(Patient donor, Patient recipient);

    public List<Patient> GetTopCompatibleDonors(int recipientId);

    public bool IsBloodMatch(BloodType? donor, BloodType receiver);

    public bool IsRhMatch(Rh? donor, Rh receiver);
}
