namespace HospitalManagement.Web.Models.Patients;

public class PatientProfileViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Cnp { get; set; } = string.Empty;
    public string BloodType { get; set; } = "N/A";
    public string Rh { get; set; } = "N/A";
    public string FormattedAllergies { get; set; } = "None";
    public string FormattedChronicConditions { get; set; } = "None";
    public bool IsHighRisk { get; set; }
    public List<MedicalRecordViewModel> MedicalRecords { get; set; } = new ();
    public int? SelectedRecordId { get; set; }
    public MedicalRecordViewModel? SelectedRecord =>
        MedicalRecords.FirstOrDefault(r => r.Id == SelectedRecordId);
}

public class MedicalRecordViewModel
{
    public int Id { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string Symptoms { get; set; } = "N/A";
    public string Diagnosis { get; set; } = "N/A";
    public int? PrescriptionId { get; set; }
}
