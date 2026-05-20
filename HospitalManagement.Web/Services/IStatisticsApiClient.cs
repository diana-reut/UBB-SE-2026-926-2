namespace HospitalManagement.Web.Services;

public interface IStatisticsApiClient
{
    Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetAgeDistributionAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetPatientGenderDistributionAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetConsultationDistributionAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetTopDiagnosesAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetMostPrescribedMedsAsync(CancellationToken cancellationToken);
}
