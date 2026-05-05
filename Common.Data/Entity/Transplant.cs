using System;
using System.ComponentModel.DataAnnotations;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

public class Transplant
{
    [Key]
    public int TransplantId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    public int? DonorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string OrganType { get; set; } = "";

    [Required]
    public DateTime RequestDate { get; set; }

    public DateTime? TransplantDate { get; set; }

    [Required]
    public TransplantStatus Status { get; set; }

    public float CompatibilityScore { get; set; }
}