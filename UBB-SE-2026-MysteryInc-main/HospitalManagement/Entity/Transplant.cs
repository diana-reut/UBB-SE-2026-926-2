using System;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

internal class Transplant
{
    public int TransplantId { get; set; }

    public int ReceiverId { get; set; }

    public int? DonorId { get; set; }

    public string OrganType { get; set; } = "";

    public DateTime RequestDate { get; set; }

    public DateTime? TransplantDate { get; set; }

    public TransplantStatus Status { get; set; }

    public float CompatibilityScore { get; set; }
}
