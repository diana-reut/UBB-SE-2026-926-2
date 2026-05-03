using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HospitalManagement.Service;

internal static class PatientValidator
{
    public static void ValidateUpdate(Patient newDetails, Patient existingPatient)
    {
        if (newDetails is null || existingPatient is null)
        {
            throw new ValidationException("Patient data cannot be null.");
        }

        if (newDetails.Cnp != existingPatient.Cnp || newDetails.Dob != existingPatient.Dob)
        {
            throw new ValidationException("Identity cannot be modified: CNP and Date of Birth must remain consistent.");
        }

        if (existingPatient.IsArchived)
        {
            throw new ValidationException("Archived patients cannot be updated unless de-archived.");
        }
    }

    public static ValidationResult ValidatePatient(Patient patient)
    {
        var result = new ValidationResult();
        // for this line above if there is an error replace with this: var result = new global::ValidationResult();

        if (patient is null)
        {
            result.AddError("Patient data cannot be null.");
            return result;
        }

        ValidateName(patient.FirstName, "First Name", result);
        ValidateName(patient.LastName, "Last Name", result);

        ValidatePhone(patient.PhoneNo, "Phone Number", isRequired: true, result);
        ValidatePhone(patient.EmergencyContact, "Emergency Contact", isRequired: false, result);

        if (ValidateCnp(patient.Cnp, result))
        {
            ValidateCnpCorrelation(patient.Cnp, patient.Sex, patient.Dob, result);
        }

        if (patient.Dob > DateTime.Now)
        {
            result.AddError("Date of Birth cannot be in the future.");
        }

        if (patient.Dod.HasValue)
        {
            if (patient.Dod.Value > DateTime.Now)
            {
                result.AddError("Date of Death cannot be in the future.");
            }

            if (patient.Dod.Value < patient.Dob)
            {
                result.AddError("Date of Death cannot be earlier than Date of Birth.");
            }
        }

        return result;
    }

    private static void ValidateName(string name, string field, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            result.AddError($"{field} must be between 1-100 characters.");
            return;
        }

        if (!Regex.IsMatch(name, @"^[\p{L}\s\-]+$"))
        {
            result.AddError($"{field} can only contain letters, spaces, and hyphens.");
        }
    }

    private static void ValidatePhone(string phone, string field, bool isRequired, ValidationResult result)
    {
        if (!isRequired && string.IsNullOrEmpty(phone))
        {
            return; // Skip validation if it's optional and empty
        }

        if (string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, @"^\+40\d{9}$"))
        {
            result.AddError($"{field} must be in format +40XXXXXXXXX.");
        }
    }

    private static bool ValidateCnp(string cnp, ValidationResult result)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
        {
            result.AddError("CNP must be exactly 13 digits.");
            return false;
        }
        else if (!"1256".Contains(cnp[0], StringComparison.Ordinal))
        {
            result.AddError("CNP must start with 1, 2, 5, or 6.");
            return false;
        }

        return true;
    }

    private static void ValidateCnpCorrelation(string cnp, Sex sex, DateTime dob, ValidationResult result)
    {
        int firstDigit = cnp[0] - '0';
        if (sex == Sex.M && firstDigit % 2 == 0)
        {
            result.AddError("CNP first digit must be odd for Male.");
        }

        if (sex == Sex.F && firstDigit % 2 != 0)
        {
            result.AddError("CNP first digit must be even for Female.");
        }

        string cnpDobPart = cnp.Substring(1, 6);
        string expectedDobPart = dob.ToString("yyMMdd", CultureInfo.InvariantCulture);
        if (cnpDobPart != expectedDobPart)
        {
            result.AddError("CNP digits 2-7 must match the Date of Birth.");
        }
    }

    public static ValidationResult ValidateMedicalHistory(MedicalHistory history, MedicalHistory? existingHistory = null)
    {
        var result = new ValidationResult();

        if (history is null)
        {
            result.AddError("Medical history data cannot be null.");
            return result;
        }

        if (history.PatientId <= 0)
        {
            result.AddError("Medical History must be associated with a valid Patient (PatientId is required).");
        }

        if (existingHistory is not null && history.PatientId != existingHistory.PatientId)
        {
            result.AddError("Patient ID cannot be modified on update. A medical history belongs strictly to its original patient.");
        }

        if (history.BloodType.HasValue && !Enum.IsDefined(history.BloodType.Value))
        {
            result.AddError("Blood Type must be exactly one of the allowed values: A, B, AB, or O.");
        }

        if (history.Rh.HasValue && !Enum.IsDefined(history.Rh.Value))
        {
            result.AddError("Rh Factor must be strictly Positive or Negative.");
        }

        if (history.ChronicConditions is not null)
        {
            int totalLength = history.ChronicConditions.Sum(c => c?.Length ?? 0);
            if (totalLength > 2000)
            {
                result.AddError("The combined description of chronic conditions cannot exceed 2000 characters.");
            }
        }

        if (history.Allergies is not null)
        {
            string[] allowedSeverities = ["Mild", "Moderate", "Severe", "Anaphylactic"];

            foreach ((Allergy Allergy, string SeverityLevel) allergy in history.Allergies)
            {
                string severity = allergy.SeverityLevel;
                if (string.IsNullOrWhiteSpace(severity) || !allowedSeverities.Contains(severity, StringComparer.OrdinalIgnoreCase))
                {
                    result.AddError($"Allergy severity '{severity}' is invalid. Allowed values: Mild, Moderate, Severe, Anaphylactic.");
                }
            }
        }

        return result;
    }
}
