namespace HospitalManagement.Web.Models.Queue;

public class QueueViewModel
{
    public List<QueueItemViewModel> ActiveVisits { get; set; } = new ();
    public string? ErrorMessage { get; set; }
}

public class QueueItemViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public int? TriageLevel { get; set; }
    public string? Specialization { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasTriageData { get; set; }
    public string? WarningMessage { get; set; }
}
