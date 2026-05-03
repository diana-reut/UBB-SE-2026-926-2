using System.Collections.Generic;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

internal class MedicalHistory
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public BloodType? BloodType { get; set; }

    public Rh? Rh { get; set; }

    public List<string> ChronicConditions { get; set; } = null!;

    public List<MedicalRecord> MedicalRecords { get; set; } = null!;

    public List<(Allergy Allergy, string SeverityLevel)> Allergies { get; set; } = null!;
}
