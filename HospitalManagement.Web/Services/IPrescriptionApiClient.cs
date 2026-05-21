using Common.Data.Entity;
using Common.Data.Integration;

namespace HospitalManagement.Web.Services;

public interface IPrescriptionApiClient
{
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page, CancellationToken cancellationToken);
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter, CancellationToken cancellationToken);
    Task<Prescription?> GetPrescriptionDetailsAsync(int id, CancellationToken cancellationToken);
}
