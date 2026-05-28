using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Patients;

public class CreatePatientViewModel : IValidatableObject
{
    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
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
    [Display(Name = "Phone number")]
    public string PhoneNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Emergency contact")]
    public string EmergencyContact { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            yield return new ValidationResult("First Name is required", new[] { nameof(FirstName) });
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            yield return new ValidationResult("Last Name is required", new[] { nameof(LastName) });
        }

        if (!IsTenDigitPhone(PhoneNo))
        {
            yield return new ValidationResult("Phone must be 10 digits", new[] { nameof(PhoneNo) });
        }

        if (!IsTenDigitPhone(EmergencyContact))
        {
            yield return new ValidationResult("Emergency contact must be 10 digits", new[] { nameof(EmergencyContact) });
        }
    }

    private static bool IsTenDigitPhone(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length == 10 && value.All(char.IsDigit);
}
