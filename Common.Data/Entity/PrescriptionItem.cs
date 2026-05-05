using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Entity;

public class PrescriptionItem
{
    [Key]
    public int PrescrItemId { get; set; }

    [Required]
    public int Prescription { get; set; }

    [Required]
    [MaxLength(200)]
    public string MedName { get; set; } = "";

    [MaxLength(50)]
    public string? Quantity { get; set; }
}