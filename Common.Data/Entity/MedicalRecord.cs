using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Data.Entity.Enums;

namespace Common.Data.Entity;

public class MedicalRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int HistoryId { get; set; }

    [Required]
    [JsonIgnore]
    public MedicalHistory History { get; set; } = null!;

    public SourceType SourceType { get; set; }

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
