using Common.Data.Entity;

namespace Common.API.Services;

public interface IBloodCompatibilityService
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
}