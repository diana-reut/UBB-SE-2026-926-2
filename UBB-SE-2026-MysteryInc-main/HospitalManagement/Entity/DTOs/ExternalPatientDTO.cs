using System;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity.DTOs;

internal class ExternalPatientDTO
{
    public string CNP { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public Sex Sex { get; set; }

    public DateTime EmergencyTimestamp { get; set; }

    public string? Injury { get; set; }
}
