using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models;

public class AuthenticationViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
