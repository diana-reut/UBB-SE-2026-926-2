namespace Common.Data.Entity.DTOs;

public class ERExaminationSummaryDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime ArrivalDateTime { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int Consciousness { get; set; }
    public int Breathing { get; set; }
    public int Bleeding { get; set; }
    public int InjuryType { get; set; }
    public int PainLevel { get; set; }
    public int DoctorId { get; set; }
    public DateTime ExamTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string AssignedDoctorName { get; set; } = string.Empty;
    public string SeverityScore => $"{Consciousness + Breathing + Bleeding + InjuryType + PainLevel} / 15";
}
