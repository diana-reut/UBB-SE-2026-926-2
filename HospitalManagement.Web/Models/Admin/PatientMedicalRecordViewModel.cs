namespace HospitalManagement.Web.Models.Admin;

public class PatientMedicalRecordViewModel
{
    public int Id { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
}
