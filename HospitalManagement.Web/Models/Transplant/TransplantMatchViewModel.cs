namespace HospitalManagement.Web.Models.Transplant;

public class TransplantMatchViewModel
{
    public int TransplantId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public float CompatibilityScore { get; set; }
    public int WaitingDays { get; set; }
}
