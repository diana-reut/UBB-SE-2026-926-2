using Common.Data.Entity;
using Common.Data.Entity.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Data.Service;

public interface IBloodCompatibilityService
{
    public int CalculateScore(Patient donor, Patient recipient);
    public List<Patient> GetTopCompatibleDonors(int recipientId);
    public Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
    public bool IsBloodMatch(BloodType? donor, BloodType receiver);
    public bool IsRhMatch(Rh? donor, Rh receiver);
}
