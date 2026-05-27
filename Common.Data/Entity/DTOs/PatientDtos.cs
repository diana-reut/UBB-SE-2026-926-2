using Common.Data.Entity.Enums;

namespace Common.Data.Entity.DTOs;

public class CreatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public Sex Sex { get; set; }
    public string PhoneNo { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsDonor { get; set; }
}

public class UpdatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public Sex Sex { get; set; }
    public string PhoneNo { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsDonor { get; set; }
    public bool Transferred { get; set; }
    public DateTime? Dod { get; set; }
    public bool IsArchived { get; set; }
}

public class ArchiveAsDeceasedDto
{
    public DateTime DeathDate { get; set; }
}

public class SearchPatientsDto
{
    public string? NamePart { get; set; }
    public string? Cnp { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Sex? Sex { get; set; }
    public bool? HasChronicCond { get; set; }
    public DateTime? LastUpdatedFrom { get; set; }
    public DateTime? LastUpdatedTo { get; set; }
    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }
}

public class CreateMedicalHistoryDto
{
    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }
    public List<string> ChronicConditions { get; set; } = new List<string>();
    public List<int> AllergyIds { get; set; } = new List<int>();
}
