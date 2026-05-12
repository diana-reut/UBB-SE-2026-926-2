using Common.Data.Entity;

namespace Common.Data.Entity.DTOs;

public sealed class BuildPoliceReportRequestDto
{
    public required int PatientId { get; set; }
}