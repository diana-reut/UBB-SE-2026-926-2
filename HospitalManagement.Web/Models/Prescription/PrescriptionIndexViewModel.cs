namespace HospitalManagement.Web.Models.Prescription;

public class PrescriptionIndexViewModel
{
    public string? SearchId { get; set; }
    public string? SearchName { get; set; }
    public string? SearchMedication { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; }
    public string? InfoMessage { get; set; }
    public List<PrescriptionListItemViewModel> Prescriptions { get; set; } = new ();
}
