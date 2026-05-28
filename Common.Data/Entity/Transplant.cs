using System;
using System.ComponentModel.DataAnnotations;
using Common.Data.Entity.Enums;

namespace Common.Data.Entity;

public class Transplant
{
    [Key]
    public int TransplantId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    public int? DonorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string OrganType { get; set; } = string.Empty;

    [Required]
    public DateTime RequestDate { get; set; }

    public DateTime? TransplantDate { get; set; }

    [Required]
    public TransplantStatus Status { get; set; }

    public float CompatibilityScore { get; set; }
}