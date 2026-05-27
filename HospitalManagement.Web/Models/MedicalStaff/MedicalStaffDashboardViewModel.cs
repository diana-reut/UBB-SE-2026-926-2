namespace HospitalManagement.Web.Models.MedicalStaff;

public class MedicalStaffDashboardViewModel
{
    public string? SearchQuery { get; set; }
    public List<PatientSearchResultViewModel> SearchResults { get; set; } = new ();
    public string? ErrorMessage { get; set; }
    public bool HasSearched { get; set; }
    public int? SelectedPatientId { get; set; }
    public PatientSearchResultViewModel? SelectedPatient =>
        SearchResults.FirstOrDefault(p => p.Id == SelectedPatientId);
}

public class PatientSearchResultViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
}
