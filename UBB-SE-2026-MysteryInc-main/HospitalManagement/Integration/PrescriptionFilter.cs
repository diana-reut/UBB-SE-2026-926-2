using System;

namespace HospitalManagement.Integration;

internal class PrescriptionFilter
{
    public int? PrescriptionId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public string? MedName { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public string? PatientName { get; set; }

    public string? DoctorName { get; set; }
}
