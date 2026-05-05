using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Entity;

public class Prescription
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecordId { get; set; }

    public List<PrescriptionItem> MedicationList { get; set; } = new();

    [MaxLength(2000)]
    public string? DoctorNotes { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(100)]
    public string PatientName { get; set; } = "Unknown";

    [Required]
    [MaxLength(100)]
    public string DoctorName { get; set; } = "Unknown";
}
