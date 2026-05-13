using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Validators;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal class AddPatientDialogViewModel
{
    private readonly IPatientProxy _patientService;

    public AddPatientDialogViewModel(IPatientProxy patientService)
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
        return SubmitPatientAsync(firstName, lastName, sex, dob, cnp, phone, emergencyContact).GetAwaiter().GetResult();
    }

    public async Task<(bool Success, string? ErrorMessage, Patient? Patient)> SubmitPatientAsync(
        string firstName, string lastName, string sex, DateTimeOffset? dob, string cnp, string phone, string emergencyContact)
    {
        var dto = new CreatePatientDto
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
            Patient created = await _patientService.CreatePatientAsync(dto);
            return (true, null, created);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message, null);
        }
    }
}
