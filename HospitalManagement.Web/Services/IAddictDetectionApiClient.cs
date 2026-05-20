using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public interface IAddictDetectionApiClient
{
    Task<List<Patient>> GetCandidatesAsync(CancellationToken cancellationToken);
    Task<string> BuildPoliceReportAsync(int patientId, CancellationToken cancellationToken);
    Task MarkPoliceNotifiedAsync(int patientId, CancellationToken cancellationToken);
}
