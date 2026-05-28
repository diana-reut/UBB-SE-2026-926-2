using System.ComponentModel.DataAnnotations;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Admin;

public class EditPatientViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    public string Cnp { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime Dob { get; set; }

    [Required]
    public Sex Sex { get; set; }

    [Required]
    [Display(Name = "Phone number")]
    public string PhoneNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Emergency contact")]
    public string EmergencyContact { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
    public bool IsDonor { get; set; }
    public bool Transferred { get; set; }
    public DateTime? Dod { get; set; }

    public bool IsDeceased => Dod.HasValue;
}
