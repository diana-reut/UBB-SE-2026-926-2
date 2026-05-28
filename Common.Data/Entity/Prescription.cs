using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Data.Entity;

public class Prescription
{
    private string? patientName;

    [Key]
    public int Id { get; set; }

    [Required]
    public int RecordId { get; set; }

    [Required]
    [JsonIgnore]
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public List<PrescriptionItem> MedicationList { get; set; } = new List<PrescriptionItem>();

    [MaxLength(2000)]
    public string? DoctorNotes { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [NotMapped]
    public string PatientName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(patientName))
            {
                return patientName;
            }

            return MedicalRecord?.History?.Patient == null
                ? string.Empty
                : $"{MedicalRecord.History.Patient.FirstName} {MedicalRecord.History.Patient.LastName}";
        }

        set => patientName = value;
    }

    [NotMapped]
    public string DoctorName { get; set; } = "Unknown";
}
