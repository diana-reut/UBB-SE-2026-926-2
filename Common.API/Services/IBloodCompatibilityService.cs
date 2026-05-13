using Common.Data.Entity;
using Common.Data.Entity.Enums;

namespace Common.API.Services;

public interface IBloodCompatibilityService
{
    int CalculateScore(Patient donor, Patient recipient);
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
    bool IsBloodMatch(BloodType? donor, BloodType receiver);
    bool IsRhMatch(Rh? donor, Rh receiver);
}
