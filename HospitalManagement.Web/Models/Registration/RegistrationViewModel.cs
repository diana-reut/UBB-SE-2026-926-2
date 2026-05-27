using System.ComponentModel.DataAnnotations;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Registration;

public class RegistrationViewModel : IValidatableObject
{
    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP must contain exactly 13 digits.")]
    public string PatientId { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);

    [Required]
    public Sex Sex { get; set; } = Sex.M;

    [Required]
    [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone must be in format 07XXXXXXXX.")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string EmergencyContact { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ChiefComplaint { get; set; } = string.Empty;

    public int? CreatedVisitId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth >= DateTime.Today)
        {
            yield return new ValidationResult("Date of birth must be in the past.", new[] { nameof(DateOfBirth) });
        }
    }
}
