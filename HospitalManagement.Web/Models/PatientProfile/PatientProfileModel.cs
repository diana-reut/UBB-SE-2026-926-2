using Common.Data.Entity;

namespace HospitalManagement.Web.Models.PatientProfile;

public class PatientProfileModel
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string? BloodType { get; set; }
    public string? Rh { get; set; }
    public string ChronicConditionsFormatted { get; set; } = "None";
    public List<string> Allergies { get; set; } = new ();
    public List<MedicalRecord> MedicalRecords { get; set; } = new ();
    public int? SelectedRecordId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
    public Common.Data.Entity.Prescription? SelectedPrescription { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
