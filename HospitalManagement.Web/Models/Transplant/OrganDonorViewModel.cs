namespace HospitalManagement.Web.Models.Transplant;

public class OrganDonorViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    public string? SelectedOrgan { get; set; }
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public List<string> Organs { get; } = new ()
    {
        "Kidney", "Heart", "Liver", "Lung", "Pancreas", "Cornea"
    };

    public List<TransplantMatchViewModel> TopMatches { get; set; } = new ();
}
