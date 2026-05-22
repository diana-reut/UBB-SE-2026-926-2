using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public interface IBloodCompatibilityApiClient
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId, CancellationToken cancellationToken);
}