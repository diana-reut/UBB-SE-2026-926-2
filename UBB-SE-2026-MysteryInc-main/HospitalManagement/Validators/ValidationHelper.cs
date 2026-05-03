using System.Linq;

namespace HospitalManagement.Validators;

internal static class ValidationHelper
{
    public static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    public static bool IsValidCnp(string cnp)
    {
        return !string.IsNullOrWhiteSpace(cnp) && cnp.Length == 13 && cnp.All(char.IsDigit);
    }

    public static bool IsValidPhone(string phone)
    {
        return !string.IsNullOrWhiteSpace(phone) && phone.Length == 10 && phone.All(char.IsDigit);
    }
}
