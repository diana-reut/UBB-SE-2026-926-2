using Common.Data.Models;

namespace Common.Data.Entity.DTOs;

public class PerformTriageRequestDto
{
    public int VisitId { get; set; }
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public DateTime TriageTime { get; set; } = DateTime.Now;
    public int Consciousness { get; set; }
    public int Breathing { get; set; }
    public int Bleeding { get; set; }
    public int InjuryType { get; set; }
    public int PainLevel { get; set; }

    public Triage_Parameters ToParameters(int triageId) =>
        new ()
        {
            TriageId = triageId,
            Consciousness = Consciousness,
            Breathing = Breathing,
            Bleeding = Bleeding,
            Injury_Type = InjuryType,
            Pain_Level = PainLevel
        };
}

public class PerformTriageResponseDto
{
    public Triage Triage { get; set; } = new ();
    public Triage_Parameters Parameters { get; set; } = new ();
}
