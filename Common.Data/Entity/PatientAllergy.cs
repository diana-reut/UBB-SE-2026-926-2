using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Entity;

public class PatientAllergy
{
    [Required]
    public int MedicalHistoryId { get; set; }

    [Required]
    public int AllergyId { get; set; }

    [Required]
    public MedicalHistory MedicalHistory { get; set; } = null!;

    [Required]
    public Allergy Allergy { get; set; } = null!;

    [Required]
    public string SeverityLevel { get; set; } = string.Empty;
}
