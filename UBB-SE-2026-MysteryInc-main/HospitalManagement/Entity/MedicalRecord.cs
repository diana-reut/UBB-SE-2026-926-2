using System;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

internal class MedicalRecord
{
    public int Id { get; set; }

    public int HistoryId { get; set; }

    public SourceType SourceType { get; set; }

    public int SourceId { get; set; }

    public int StaffId { get; set; }

    public string? Symptoms { get; set; }

    public string? Diagnosis { get; set; }

    public DateTime ConsultationDate { get; set; }

    public int? PrescriptionId { get; set; }

    public decimal BasePrice { get; set; }

    public decimal FinalPrice { get; set; }

    public int? DiscountApplied { get; set; }

    public bool PoliceNotified { get; set; }

    public int? TransplantId { get; set; }
}
