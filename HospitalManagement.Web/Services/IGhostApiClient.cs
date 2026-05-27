namespace HospitalManagement.Web.Services;

public record GhostStatusDto(bool exorcismTriggered, int sightingCount);

public interface IGhostApiClient
{
    Task<GhostStatusDto> ReportSightingAsync(CancellationToken cancellationToken);
    Task<GhostStatusDto> GetExorcismStatusAsync(CancellationToken cancellationToken);
}
