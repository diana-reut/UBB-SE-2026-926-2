namespace HospitalManagement.Web.Models.Prescription;

public class PrescriptionFeedViewModel
{
    public string? SearchIdText { get; set; }
    public string? SearchName { get; set; }
    public string? SearchMedication { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? InfoMessage { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int? InitialPrescriptionId { get; set; }
    public int? ReturnPatientId { get; set; }
    public List<PrescriptionCardViewModel> Prescriptions { get; set; } = new ();
}

public class PrescriptionCardViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorNotes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<PrescriptionItemViewModel> Items { get; set; } = new ();
    public string MedName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
