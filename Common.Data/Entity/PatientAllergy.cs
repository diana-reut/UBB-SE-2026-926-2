using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagement.Entity;

namespace Common.Data.Entity
{
    public class PatientAllergy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MedicalHistoryId { get; set; }
        
        [Required]
        public Allergy Allergy { get; set; }

        public string SeverityLevel { get; set; }
    }
}
