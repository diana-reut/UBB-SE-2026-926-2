using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Entity;

public class MedicalRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public MedicalHistory History { get; set; }

    public SourceType SourceType { get; set; }

    // What are these id's linked to?
    public int SourceId { get; set; }

    public int StaffId { get; set; }
    [MaxLength(200)]
    public string? Symptoms { get; set; }
    [MaxLength(100)]
    public string? Diagnosis { get; set; }

    [Required]
    public DateTime ConsultationDate { get; set; }

    public Prescription? Prescription { get; set; }

    [Required]
    public decimal BasePrice { get; set; }

    [Required]
    public decimal FinalPrice { get; set; }

    public int? DiscountApplied { get; set; }
    [Required]
    public bool PoliceNotified { get; set; }

    public int? TransplantId { get; set; }
}
