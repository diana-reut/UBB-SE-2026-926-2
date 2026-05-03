using System;
using System.Collections.Generic;

namespace ERManagementSystem.Models
{
    public class Examination
    {
        public int Exam_ID { get; set; }
        public int Visit_ID { get; set; }
        public int Doctor_ID { get; set; }
        public DateTime Exam_Time { get; set; }
        public int Room_ID { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Examination()
        {
            Exam_Time = DateTime.Now;
        }

        public Examination(int examID, int visitId, int doctorID, DateTime examTime, int roomID, string notes)
        {
            Exam_ID = examID;
            Visit_ID = visitId;
            Doctor_ID = doctorID;
            Exam_Time = examTime;
            Room_ID = roomID;
            Notes = notes;
        }
        // Examination records are already in schema.sql
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (Exam_ID <= 0)
            {
                errors.Add("Exam_ID must be a positive integer.");
            }

            if (Visit_ID <= 0)
            {
                errors.Add("Visit_ID is required and must be valid.");
            }

            if (Doctor_ID <= 0)
            {
                errors.Add("Doctor_ID is required.");
            }

            if (Room_ID <= 0)
            {
                errors.Add("Room_ID is required.");
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                errors.Add("Notes must not be empty");
            }

            if (Exam_Time == default)
            {
                errors.Add("Examination date and time is required");
            }

            return errors.Count == 0;
        }

        public override string ToString() =>
            $"[Examination {Exam_ID}] [Visit {Visit_ID}] Doctor: {Doctor_ID}  | Examination Time: {Exam_Time:yyyy-MM-dd HH:mm} | Room: {Room_ID} | Notes: {Notes}";
    }
}