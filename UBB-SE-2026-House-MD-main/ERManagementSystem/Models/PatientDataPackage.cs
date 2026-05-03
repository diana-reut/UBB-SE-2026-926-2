using System;

namespace ERManagementSystem.Models
{
    /// <summary>
    /// Task 6.3 - Data Transfer Object that packages all required patient data
    /// for sending to the external Patient Management system (Feature 8).
    ///
    /// Fields are assembled via a hand-written JOIN query across:
    /// Patient, ER_Visit, Triage, Triage_Parameters, Examination
    /// </summary>
    public class PatientDataPackage
    {
        // --- From Patient table ---
        public string CNP { get; set; } = string.Empty;
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public DateTime Date_of_Birth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Emergency_Contact { get; set; } = string.Empty;

        // --- From ER_Visit table ---
        public int Visit_ID { get; set; }
        public DateTime Arrival_date_time { get; set; }
        public string Chief_Complaint { get; set; } = string.Empty;

        // --- From Triage table ---
        public int Triage_Level { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public int Nurse_ID { get; set; }

        // --- From Triage_Parameters table ---
        public int Consciousness { get; set; }
        public int Breathing { get; set; }
        public int Bleeding { get; set; }
        public int Injury_Type { get; set; }
        public int Pain_Level { get; set; }

        // --- From Examination table ---
        public DateTime? Exam_Time { get; set; }
        public string? Notes { get; set; }
        public int? Doctor_ID { get; set; }
    }
}