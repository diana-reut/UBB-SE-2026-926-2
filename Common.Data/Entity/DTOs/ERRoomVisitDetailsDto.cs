using Common.Data.Models;

namespace Common.Data.Entity.DTOs;

public class ERRoomVisitDetailsDto
{
    public ER_Visit? Visit { get; set; }
    public Patient? Patient { get; set; }
    public Triage? Triage { get; set; }
}
