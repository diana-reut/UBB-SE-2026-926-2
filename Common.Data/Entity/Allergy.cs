using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Entity;

public class Allergy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AllergyName { get; set; } = "";

    [MaxLength(50)]
    public string? AllergyType { get; set; }

    [MaxLength(50)]
    public string? AllergyCategory { get; set; }

}
