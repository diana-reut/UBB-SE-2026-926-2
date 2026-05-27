namespace HospitalManagement.Web.Models.MedicalStaff;

public class PrescriptionDetailsViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorNotes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<PrescriptionItemViewModel> Items { get; set; } = new ();
    public int ReturnPatientId { get; set; }
}

public class PrescriptionItemViewModel
{
    public string MedName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
