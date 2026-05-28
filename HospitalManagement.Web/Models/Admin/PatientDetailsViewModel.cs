namespace HospitalManagement.Web.Models.Admin;

public class PatientDetailsViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public string? BloodType { get; set; }
    public string? Rh { get; set; }
    public string ChronicConditions { get; set; } = "None";
    public List<string> Allergies { get; set; } = new ();
    public List<PatientMedicalRecordViewModel> MedicalRecords { get; set; } = new ();
}
