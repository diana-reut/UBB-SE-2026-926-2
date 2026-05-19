namespace HospitalManagement.Web.Models.Patients;

public class PatientListItemViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsDeceased { get; set; }
}
