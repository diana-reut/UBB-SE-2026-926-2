using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Common.Data.Entity;
using Common.Data.Entity.Enums;

namespace Common.Data.Entity;

public class MedicalHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    [JsonIgnore]
    public Patient Patient { get; set; } = null!;

    public BloodType? BloodType { get; set; }

    public Rh? Rh { get; set; }

    public List<string> ChronicConditions { get; set; } = new List<string>();

    public List<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public List<PatientAllergy> PatientAllergies { get; set; } = new List<PatientAllergy>();

    [NotMapped]
    public List<(Allergy Allergy, string SeverityLevel)> Allergies
    {
        get => (PatientAllergies ?? new List<PatientAllergy>())
            .Where(pa => pa?.Allergy is not null)
            .Select(pa => (pa.Allergy, pa.SeverityLevel))
            .ToList();
        set
        {
            PatientAllergies = value?
                .Where(item => item.Allergy is not null)
                .Select(item => new PatientAllergy
                {
                    Allergy = item.Allergy,
                    AllergyId = item.Allergy.Id,
                    SeverityLevel = item.SeverityLevel ?? string.Empty,
                    MedicalHistoryId = Id,
                })
                .ToList() ?? new List<PatientAllergy>();
        }
    }
}
