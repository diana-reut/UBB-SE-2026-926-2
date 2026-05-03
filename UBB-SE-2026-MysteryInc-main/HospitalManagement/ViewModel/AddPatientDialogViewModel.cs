using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Service;
using HospitalManagement.Validators;
using System;

namespace HospitalManagement.ViewModel;

internal class AddPatientDialogViewModel
{
    private readonly IPatientService _patientService;

    public AddPatientDialogViewModel(IPatientService patientService)
    {
        _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
    }

    public record FormValidationResult(
        bool FirstNameValid,
        bool LastNameValid,
        bool CnpValid,
        bool PhoneValid,
        bool EmergencyValid)
    {
        public bool IsValid => FirstNameValid && LastNameValid && CnpValid && PhoneValid && EmergencyValid;
    }

    public static FormValidationResult ValidateForm(string firstName, string lastName, string cnp, string phone, string emergencyContact)
    {
        return new FormValidationResult(
            ValidationHelper.IsValidName(firstName),
            ValidationHelper.IsValidName(lastName),
            ValidationHelper.IsValidCnp(cnp),
            ValidationHelper.IsValidPhone(phone),
            ValidationHelper.IsValidPhone(emergencyContact)
        );
    }

    public (bool Success, string? ErrorMessage, Patient? Patient) SubmitPatient(
        string firstName, string lastName, string sex, DateTimeOffset? dob, string cnp, string phone, string emergencyContact)
    {
        Patient data = new()
        {
            FirstName = firstName,
            LastName = lastName,
            Sex = Enum.Parse<Sex>(sex),
            Dob = dob?.DateTime ?? DateTime.Now,
            Cnp = cnp,
            PhoneNo = phone,
            EmergencyContact = emergencyContact,
        };

        try
        {
            Patient created = _patientService.CreatePatient(data);
            return (true, null, created);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message, null);
        }
    }
}
