using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Entity;

public class Prescription
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecordId { get; set; }

    [Required]
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public List<PrescriptionItem> MedicationList { get; set; } = [];

    [MaxLength(2000)]
    public string? DoctorNotes { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [NotMapped]
    public string PatientName =>
        MedicalRecord?.History?.Patient == null
        ? ""
        : $"{MedicalRecord.History.Patient.FirstName} " +
          $"{MedicalRecord.History.Patient.LastName}";

    [NotMapped]
    public string DoctorName { get; set; } = "Unknown";
}
