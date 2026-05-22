namespace HospitalManagement.Web.Models.BloodCompatibility;

public class DonorMatchViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Cnp { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string RhFactor { get; set; } = string.Empty;
    public int Score { get; set; }
}