namespace HospitalManagement.Web.Models.Queue;

public class QueueViewModel
{
    public List<QueueItemViewModel> ActiveVisits { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public class QueueItemViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
