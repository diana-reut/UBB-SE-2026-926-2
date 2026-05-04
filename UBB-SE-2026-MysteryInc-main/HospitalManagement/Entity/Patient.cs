using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

internal class Patient
{
    public int Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Cnp { get; set; } = "";

    public DateTime Dob { get; set; }

    public DateTime? Dod { get; set; }

    public Sex Sex { get; set; }

    public string PhoneNo { get; set; } = "";

    public string EmergencyContact { get; set; } = "";

    public bool IsArchived { get; set; }

    public bool IsDonor { get; set; }

    public bool Transferred { get; set; }

    public MedicalHistory? MedicalHistory { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string Patient_ID
    {
        get => Cnp;
        set => Cnp = value;
    }

    public string First_Name
    {
        get => FirstName;
        set => FirstName = value;
    }

    public string Last_Name
    {
        get => LastName;
        set => LastName = value;
    }

    public DateTime Date_of_Birth
    {
        get => Dob;
        set => Dob = value;
    }

    public string Gender
    {
        get => Sex == Sex.F ? "Female" : "Male";
        set => Sex = value == "Female" ? Sex.F : Sex.M;
    }

    public string Phone
    {
        get => PhoneNo;
        set => PhoneNo = value;
    }

    public string Emergency_Contact
    {
        get => EmergencyContact;
        set => EmergencyContact = value;
    }

    public static readonly IReadOnlyList<string> AllowedGenders =
        new[] { "Male", "Female" };

    public int GetAge()
    {
        DateTime today = DateTime.Today;
        int age = today.Year - Dob.Year;
        if (Dob.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public bool IsDeceased => Dod.HasValue;

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Patient_ID))
        {
            errors.Add("Patient ID (CNP) is required.");
        }
        else if (Patient_ID.Length != 13 || !Patient_ID.All(char.IsDigit))
        {
            errors.Add("Patient ID (CNP) must be exactly 13 digits.");
        }

        if (string.IsNullOrWhiteSpace(First_Name))
        {
            errors.Add("First name is required.");
        }
        else if (First_Name.Length > 100)
        {
            errors.Add("First name must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(Last_Name))
        {
            errors.Add("Last name is required.");
        }
        else if (Last_Name.Length > 100)
        {
            errors.Add("Last name must not exceed 100 characters.");
        }

        if (Date_of_Birth == default)
        {
            errors.Add("Date of birth is required.");
        }
        else if (Date_of_Birth >= DateTime.Today)
        {
            errors.Add("Date of birth must be in the past.");
        }

        if (string.IsNullOrWhiteSpace(Gender))
        {
            errors.Add("Gender is required.");
        }
        else if (!AllowedGenders.Contains(Gender))
        {
            errors.Add($"Gender must be one of: {string.Join(", ", AllowedGenders)}.");
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            errors.Add("Phone number is required.");
        }
        else if (Phone.Length > 20)
        {
            errors.Add("Phone number must not exceed 20 characters.");
        }

        if (string.IsNullOrWhiteSpace(Emergency_Contact))
        {
            errors.Add("Emergency contact is required.");
        }
        else if (Emergency_Contact.Length > 255)
        {
            errors.Add("Emergency contact must not exceed 255 characters.");
        }

        return errors.Count == 0;
    }
}
