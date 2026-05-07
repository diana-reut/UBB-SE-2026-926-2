using System;

namespace Common.Data.Models
{
    public class Triage
    {
        public int Triage_ID { get; set; }
        public int Visit_ID { get; set; }
        public int Triage_Level { get; set; } = 5;
        public string Specialization { get; set; } = string.Empty;
        public int Nurse_ID { get; set; }
        public DateTime Triage_Time { get; set; } = DateTime.Now;
    }
}
