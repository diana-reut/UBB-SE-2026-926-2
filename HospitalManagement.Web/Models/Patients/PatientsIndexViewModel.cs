using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Patients;

public class PatientsIndexViewModel
{
    public string? SearchQuery { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public Sex? Sex { get; set; }
    public List<PatientListItemViewModel> Patients { get; set; } = new ();
}
