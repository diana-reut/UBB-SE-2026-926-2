namespace HospitalManagement.Web.Models.Pharmacist;

public class PoliceAlertViewModel
{
    public string ReportText { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public bool AlreadySent { get; set; }
}
