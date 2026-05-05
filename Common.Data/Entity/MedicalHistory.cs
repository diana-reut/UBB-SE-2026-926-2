using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;


public class MedicalHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Patient Patient { get; set; }

    public BloodType? BloodType { get; set; }

    public Rh? Rh { get; set; }

    public List<string> ChronicConditions { get; set; } = null!;

    public List<MedicalRecord> MedicalRecords { get; set; } = null!;

    public List<(Allergy Allergy, string SeverityLevel)> Allergies { get; set; } = null!;
}
