using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

public class MedicalHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public Patient Patient { get; set; } = null!;

    public BloodType? BloodType { get; set; }

    public Rh? Rh { get; set; }

    public List<string> ChronicConditions { get; set; } = [];

    public List<MedicalRecord> MedicalRecords { get; set; } = [];

    public List<PatientAllergy> PatientAllergies { get; set; } = [];

    [NotMapped]
    public List<(Allergy Allergy, string SeverityLevel)> Allergies
    {
        get => [.. PatientAllergies.Select(pa => (pa.Allergy, pa.SeverityLevel))];
        set
        {
            PatientAllergies = value?
                .Select(item => new PatientAllergy
                {
                    Allergy = item.Allergy,
                    AllergyId = item.Allergy.Id,
                    SeverityLevel = item.SeverityLevel,
                    MedicalHistoryId = Id,
                })
                .ToList() ?? [];
        }
    }
}
