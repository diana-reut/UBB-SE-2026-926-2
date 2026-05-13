using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Common.Data.Entity;

public class PatientAllergy
{
    [Required]
    public int MedicalHistoryId { get; set; }

    [Required]
    public int AllergyId { get; set; }

    [Required]
    [JsonIgnore]
    public MedicalHistory MedicalHistory { get; set; } = null!;

    [Required]
    public Allergy Allergy { get; set; } = null!;

    [Required]
    public string SeverityLevel { get; set; } = string.Empty;
}
