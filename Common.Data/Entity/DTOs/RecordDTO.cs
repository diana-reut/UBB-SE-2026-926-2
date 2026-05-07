using System;
using Common.Data.Entity.Enums;

namespace Common.Data.Entity.DTOs;

public class RecordDTO
{
    public int ExternalRecordId { get; set; }

    public string Symptoms { get; set; } = "";

    public string TemporaryDiagnosis { get; set; } = "";

    public string PrescribedMeds { get; set; } = "";

    public DateTime ConsultationDate { get; set; }

    public SourceType SourceType { get; set; }
}
