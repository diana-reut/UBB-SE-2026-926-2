namespace HospitalManagement.Web.Models.Pharmacist;

public class AddictCandidateViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? ChronicConditions { get; set; }
    public bool IsPoliceNotified { get; set; }
}
