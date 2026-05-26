namespace HospitalManagement.Web.Services;

public record GhostStatusDto(bool ExorcismTriggered, int SightingCount);

public interface IGhostApiClient
{
    Task<GhostStatusDto> ReportSightingAsync(CancellationToken cancellationToken);
    Task<GhostStatusDto> GetExorcismStatusAsync(CancellationToken cancellationToken);
}
