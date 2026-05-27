using System;
using Common.Data.Entity.Enums;

namespace Common.Data.Entity.DTOs;

public class RecordDTO
{
    public int ExternalRecordId { get; set; }

    public string Symptoms { get; set; } = string.Empty;

    public string TemporaryDiagnosis { get; set; } = string.Empty;

    public string PrescribedMeds { get; set; } = string.Empty;

    public DateTime ConsultationDate { get; set; }

    public SourceType SourceType { get; set; }
}
