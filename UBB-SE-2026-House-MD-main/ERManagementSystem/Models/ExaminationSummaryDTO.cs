using System;

namespace ERManagementSystem.Models
{
    public class ExaminationSummaryDTO
    {
        // Patient Info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Visit Info
        public DateTime ArrivalDateTime { get; set; }
        public string ChiefComplaint { get; set; } = string.Empty;

        // Triage Info
        public int TriageLevel { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public int Consciousness { get; set; }
        public int Breathing { get; set; }
        public int Bleeding { get; set; }
        public int InjuryType { get; set; }
        public int PainLevel { get; set; }

        // Examination Info
        public int DoctorId { get; set; }
        public DateTime ExamTime { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Joined App-Level Info (Requires separate resolution)
        public string AssignedDoctorName { get; set; } = string.Empty;
        public string SeverityScore => $"{Consciousness + Breathing + Bleeding + InjuryType + PainLevel} / 15";
    }
}
