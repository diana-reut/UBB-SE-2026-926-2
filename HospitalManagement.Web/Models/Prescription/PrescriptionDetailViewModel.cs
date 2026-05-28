namespace HospitalManagement.Web.Models.Prescription;

public class PrescriptionDetailViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DoctorNotes { get; set; } = "No notes provided";
    public List<PrescriptionItemViewModel> Medications { get; set; } = new ();

    // Set when navigating from the consultation details page; used to render
    // a "Back to Consultation" button that returns the user where they came from.
    public int? ReturnPatientId { get; set; }
    public int? ReturnRecordId { get; set; }
    public bool HasConsultationReturn => ReturnPatientId.HasValue && ReturnRecordId.HasValue;
}

public class PrescriptionItemViewModel
{
    public string MedName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}
