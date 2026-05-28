using System.Collections.Generic;
using Common.Data.Entity;

namespace Common.Data.Entity.DTOs;

public class RecordExportDataDto
{
    public MedicalRecord Record { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Prescription? Prescription { get; set; }
    public List<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
