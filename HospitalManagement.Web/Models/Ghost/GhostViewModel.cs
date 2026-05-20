namespace HospitalManagement.Web.Models.Ghost;

public class GhostViewModel
{
    public bool ExorcismTriggered { get; set; }
    public int SightingCount { get; set; }
    public string? StatusMessage { get; set; }
    public DateTime LastRefreshed { get; set; }

    public static GhostViewModel FromModel(GhostModel model)
    {
        return new GhostViewModel
        {
            ExorcismTriggered = model.ExorcismTriggered,
            SightingCount = model.SightingCount,
            StatusMessage = model.StatusMessage,
            LastRefreshed = model.LastRefreshed,
        };
    }
}
