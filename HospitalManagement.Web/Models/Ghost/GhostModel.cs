namespace HospitalManagement.Web.Models.Ghost;

public class GhostModel
{
    public bool ExorcismTriggered { get; set; }
    public int SightingCount { get; set; }
    public string? StatusMessage { get; set; }
    public DateTime LastRefreshed { get; set; } = DateTime.UtcNow;
}
