using HospitalManagement.Entity;

namespace HospitalManagement.View;

// Simple wrapper class for allergy entries
internal class AllergyEntry
{
    public Allergy Allergy { get; set; } = null!;

    public string Severity { get; set; } = null!;
}
