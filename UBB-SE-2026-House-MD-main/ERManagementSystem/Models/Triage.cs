using System;

namespace ERManagementSystem.Models
{
    public class Triage
    {
        public int Triage_ID { get; set; }
        public int Visit_ID { get; set; }
        public int Triage_Level { get; set; } = 5;
        public string Specialization { get; set; } = string.Empty;
        public int Nurse_ID { get; set; }
        public DateTime Triage_Time { get; set; }

        public Triage()
        {
            Triage_Time = DateTime.Now;
        }

        public Triage(int triageId, int visitId, int triageLevel, string specialization, int nurseId, DateTime triageTime)
        {
            Triage_ID = triageId;
            Visit_ID = visitId;
            Triage_Level = triageLevel;
            Specialization = specialization;
            Nurse_ID = nurseId;
            Triage_Time = triageTime;
        }
    }
}
