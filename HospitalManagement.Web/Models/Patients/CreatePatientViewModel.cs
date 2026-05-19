using Common.Data.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models.Patients;

public class CreatePatientViewModel
{
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
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP must contain exactly 13 digits.")]
    public string Cnp { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime Dob { get; set; } = DateTime.Today.AddYears(-18);

    [Required]
    public Sex Sex { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Emergency contact")]
    public string EmergencyContact { get; set; } = string.Empty;

}
