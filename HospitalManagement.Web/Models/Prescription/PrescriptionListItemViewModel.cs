namespace HospitalManagement.Web.Models.Prescription;

public class PrescriptionListItemViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MedicationCount { get; set; }
}
