namespace Common.Data.Entity.DTOs;

public class ERTransferEligibleVisitDto
{
    public int Visit_ID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Chief_Complaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Transferred { get; set; }
}
