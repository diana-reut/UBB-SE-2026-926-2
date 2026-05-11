using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Data.Entity;

public class PrescriptionItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PrescriptionId { get; set; }

    [ForeignKey(nameof(PrescriptionId))]
    [JsonIgnore]
    public Prescription Prescription { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string MedName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Quantity { get; set; }

    [NotMapped]
    public int PrescrItemId
    {
        get => Id;
        set => Id = value;
    }
}
