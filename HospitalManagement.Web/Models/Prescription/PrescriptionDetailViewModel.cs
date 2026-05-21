namespace HospitalManagement.Web.Models.Prescription;

public class PrescriptionDetailViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DoctorNotes { get; set; } = "No notes provided";
    public List<PrescriptionItemViewModel> Medications { get; set; } = [];
}

public class PrescriptionItemViewModel
{
    public string MedName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}
