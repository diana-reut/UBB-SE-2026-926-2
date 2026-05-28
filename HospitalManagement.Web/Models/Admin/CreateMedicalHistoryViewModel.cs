using System.ComponentModel.DataAnnotations;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Admin;

public class CreateMedicalHistoryViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    [Display(Name = "Blood type")]
    public BloodType BloodType { get; set; } = BloodType.A;

    [Display(Name = "RH factor")]
    public Rh Rh { get; set; } = Rh.Positive;

    [Display(Name = "Chronic conditions")]
    public string ChronicConditionsText { get; set; } = string.Empty;

    public List<int> AllergyIds { get; set; } = new ();

    public List<AllergyOptionViewModel> AvailableAllergies { get; set; } = new ();
}

public class AllergyOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
