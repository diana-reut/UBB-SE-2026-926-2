namespace HospitalManagement.Web.Models.Transfer;

public class TransferViewModel
{
    public int? SelectedVisitId { get; set; }
    public List<TransferVisitViewModel> EligibleVisits { get; set; } = new ();
    public List<TransferLogItemViewModel> TransferLogs { get; set; } = new ();
    public bool CanRetry { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TransferVisitViewModel
{
    public int VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Transferred { get; set; }
}

public class TransferLogItemViewModel
{
    public int TransferId { get; set; }
    public int VisitId { get; set; }
    public DateTime TransferTime { get; set; }
    public string TargetSystem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
