using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IBloodCompatibilityService
{
    public int CalculateScore(Patient donor, Patient recipient);
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
    public bool IsBloodMatch(BloodType? donor, BloodType receiver);

    public bool IsRhMatch(Rh? donor, Rh receiver);
}
