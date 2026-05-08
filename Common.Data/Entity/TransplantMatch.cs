using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Common.Data.Entity;

/// <summary>
/// View model representation of a transplant match for display in the DataGrid.
/// </summary>
public class TransplantMatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int TransplantId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReceiverName { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string BloodType { get; set; } = null!;

    [Required]
    public float CompatibilityScore { get; set; }

    [Required]
    public DateTime RequestDate { get; set; }

    [Required]
    public int WaitingDays { get; set; }
}