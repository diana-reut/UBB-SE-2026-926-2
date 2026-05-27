using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entity;

public class Allergy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AllergyName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AllergyType { get; set; }

    [MaxLength(50)]
    public string? AllergyCategory { get; set; }

    [NotMapped]
    public int AllergyId
    {
        get => Id;
        set => Id = value;
    }
}
