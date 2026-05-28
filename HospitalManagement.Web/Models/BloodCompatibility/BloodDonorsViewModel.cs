namespace HospitalManagement.Web.Models.BloodCompatibility;

public class BloodDonorsViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public List<DonorMatchViewModel> Donors { get; set; } = new ();
}
