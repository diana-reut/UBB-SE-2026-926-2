using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Service;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class PatientValidatorTests
{

    [TestMethod]
    public void ValidateUpdate_NullNewDetails_ThrowsValidationException()
    {
        var existing = new Patient();
        Assert.Throws<ValidationException>(() => PatientValidator.ValidateUpdate(null!, existing));
    }

    [TestMethod]
    public void ValidateUpdate_NullExistingPatient_ThrowsValidationException()
    {
        var newDetails = new Patient();
        Assert.Throws<ValidationException>(() => PatientValidator.ValidateUpdate(newDetails, null!));
    }

    [TestMethod]
    public void ValidateUpdate_CnpMismatch_ThrowsValidationException()
    {
        var existing = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1) };
        var newDetails = new Patient { Cnp = "2900101123456", Dob = new DateTime(1990, 1, 1) };

        Assert.Throws<ValidationException>(() => PatientValidator.ValidateUpdate(newDetails, existing));
    }

    [TestMethod]
    public void ValidateUpdate_DobMismatch_ThrowsValidationException()
    {
        var existing = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1) };
        var newDetails = new Patient { Cnp = "1900101123456", Dob = new DateTime(1995, 5, 5) };

        Assert.Throws<ValidationException>(() => PatientValidator.ValidateUpdate(newDetails, existing));
    }

    [TestMethod]
    public void ValidateUpdate_ArchivedPatient_ThrowsValidationException()
    {
        var existing = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1), IsArchived = true };
        var newDetails = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1) };

        Assert.Throws<ValidationException>(() => PatientValidator.ValidateUpdate(newDetails, existing));
    }

    [TestMethod]
    public void ValidateUpdate_ValidData_DoesNotThrow()
    {
        var existing = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1), IsArchived = false };
        var newDetails = new Patient { Cnp = "1900101123456", Dob = new DateTime(1990, 1, 1) };

        PatientValidator.ValidateUpdate(newDetails, existing);
    }

    [TestMethod]
    public void ValidatePatient_NullPatient_ReturnsError()
    {
        var result = PatientValidator.ValidatePatient(null!);
        Assert.IsTrue(HasError(result, "Patient data cannot be null."));
    }

    [TestMethod]
    public void ValidatePatient_NameNull_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.FirstName = null!;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "must be between 1-100 characters"));
    }

    [TestMethod]
    public void ValidatePatient_NameEmpty_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.FirstName = "";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "must be between 1-100 characters"));
    }

    [TestMethod]
    public void ValidatePatient_NameWhitespace_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.FirstName = "   ";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "must be between 1-100 characters"));
    }

    [TestMethod]
    public void ValidatePatient_NameTooLong_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.LastName = new string('A', 101);
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Last Name must be between 1-100 characters"));
    }

    [TestMethod]
    public void ValidatePatient_NameInvalidRegex_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.FirstName = "John@Doe";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "can only contain letters, spaces, and hyphens"));
    }

    [TestMethod]
    public void ValidatePatient_PhoneNull_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.PhoneNo = null!;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Phone Number must be in format"));
    }

    [TestMethod]
    public void ValidatePatient_PhoneEmpty_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.PhoneNo = "";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Phone Number must be in format"));
    }

    [TestMethod]
    public void ValidatePatient_PhoneInvalidRegex_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.PhoneNo = "0712345678"; 
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Phone Number must be in format"));
    }

    [TestMethod]
    public void ValidatePatient_EmergencyContactNull_IsValid()
    {
        var patient = GetBaseValidPatient();
        patient.EmergencyContact = null;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsFalse(HasError(result, "Emergency Contact must be in format"));
    }

    [TestMethod]
    public void ValidatePatient_EmergencyContactEmpty_IsValid()
    {
        var patient = GetBaseValidPatient();
        patient.EmergencyContact = "";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsFalse(HasError(result, "Emergency Contact must be in format"));
    }

    [TestMethod]
    public void ValidatePatient_EmergencyContactInvalidRegex_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.EmergencyContact = "0712345678"; 
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Emergency Contact must be in format"));
    }


    [TestMethod]
    public void ValidatePatient_FutureDob_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Dob = DateTime.Now.AddDays(1);
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Date of Birth cannot be in the future"));
    }

    [TestMethod]
    public void ValidatePatient_FutureDod_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Dod = DateTime.Now.AddDays(2);
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Date of Death cannot be in the future"));
    }

    [TestMethod]
    public void ValidatePatient_ValidDod_NoErrors()
    {
        var patient = GetBaseValidPatient();
        patient.Dob = new DateTime(1990, 1, 1);

        patient.Dod = new DateTime(2020, 1, 1);

        var result = PatientValidator.ValidatePatient(patient);

        Assert.IsFalse(HasError(result, "Date of Death cannot be in the future."));
        Assert.IsFalse(HasError(result, "Date of Death cannot be earlier than Date of Birth."));
    }

    [TestMethod]
    public void ValidatePatient_DodBeforeDob_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Dob = new DateTime(1990, 1, 1);
        patient.Dod = new DateTime(1989, 1, 1);
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "Date of Death cannot be earlier than Date of Birth"));
    }


    [TestMethod]
    public void ValidatePatient_CnpNull_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = null!;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP must be exactly 13 digits"));
    }

    [TestMethod]
    public void ValidatePatient_CnpWrongLength_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "12345";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP must be exactly 13 digits"));
    }

    [TestMethod]
    public void ValidatePatient_CnpContainsLetters_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "123456789012A";
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP must be exactly 13 digits"));
    }

    [TestMethod]
    public void ValidatePatient_CnpInvalidStartDigit_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "3900101123456"; 
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP must start with 1, 2, 5, or 6"));
    }

    [TestMethod]
    public void ValidatePatient_CnpMaleEvenDigit_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "2900101123456"; 
        patient.Sex = Sex.M;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP first digit must be odd for Male"));
    }

    [TestMethod]
    public void ValidatePatient_CnpFemaleOddDigit_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "1900101123456"; 
        patient.Sex = Sex.F;
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP first digit must be even for Female"));
    }

    [TestMethod]
    public void ValidatePatient_CnpDobMismatch_ReturnsError()
    {
        var patient = GetBaseValidPatient();
        patient.Cnp = "1900101123456"; 
        patient.Dob = new DateTime(1995, 5, 5);
        var result = PatientValidator.ValidatePatient(patient);
        Assert.IsTrue(HasError(result, "CNP digits 2-7 must match the Date of Birth"));
    }


    [TestMethod]
    public void ValidateMedicalHistory_NullHistory_ReturnsError()
    {
        var result = PatientValidator.ValidateMedicalHistory(null!);
        Assert.IsTrue(HasError(result, "Medical history data cannot be null."));
    }

    [TestMethod]
    public void ValidateMedicalHistory_PatientIdZero_ReturnsError()
    {
        var history = new MedicalHistory { PatientId = 0 };
        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "PatientId is required"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_PatientIdNegative_ReturnsError()
    {
        var history = new MedicalHistory { PatientId = -1 };
        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "PatientId is required"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_ExistingPatientMismatch_ReturnsError()
    {
        var newHistory = new MedicalHistory { PatientId = 1 };
        var oldHistory = new MedicalHistory { PatientId = 2 };

        var result = PatientValidator.ValidateMedicalHistory(newHistory, oldHistory);
        Assert.IsTrue(HasError(result, "Patient ID cannot be modified"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_InvalidEnums_ReturnsErrors()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            BloodType = (BloodType)999,
            Rh = (Rh)999
        };

        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "Blood Type must be exactly one of the allowed values"));
        Assert.IsTrue(HasError(result, "Rh Factor must be strictly Positive or Negative"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_ChronicConditions_Exceeds2000_ReturnsError()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            ChronicConditions = new List<string> { null!, new string('A', 2001) }
        };

        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "cannot exceed 2000 characters"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_Allergies_SeverityNull_ReturnsError()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            Allergies = new List<(Allergy, string)> { (new Allergy(), null!) }
        };

        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "Allergy severity"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_Allergies_SeverityEmpty_ReturnsError()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            Allergies = new List<(Allergy, string)> { (new Allergy(), "") }
        };

        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "Allergy severity"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_Allergies_SeverityInvalidWord_ReturnsError()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            Allergies = new List<(Allergy, string)> { (new Allergy(), "SuperSevere") }
        };

        var result = PatientValidator.ValidateMedicalHistory(history);
        Assert.IsTrue(HasError(result, "Allergy severity"));
    }

    [TestMethod]
    public void ValidateMedicalHistory_Valid_NoErrors()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            Allergies = new List<(Allergy, string)> { (new Allergy(), "Mild") }
        };

        var result = PatientValidator.ValidateMedicalHistory(history);

        dynamic dynResult = result;
        Assert.AreEqual(0, Enumerable.Count(dynResult.Errors));
    }

    private Patient GetBaseValidPatient()
    {
        return new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNo = "+40123456789",
            Cnp = "1900101123456",
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };
    }

    private bool HasError(object validationResult, string expectedSubstring)
    {
        dynamic result = validationResult;
        foreach (var error in result.Errors)
        {
            if (error?.ToString()?.Contains(expectedSubstring) == true)
            {
                return true;
            }
        }
        return false;
    }


}