using System;
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

    public MedicalHistory? MedicalHistory { get; set; }

    public string FullName => $"{FirstName} {LastName}";

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
}
