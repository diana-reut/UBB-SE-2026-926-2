using System;

namespace ERManagementSystem.Models
{
    /// <summary>
    /// Flat display model for QueueView DataGrid.
    /// Wraps the (ER_Visit, Triage) tuple into bindable properties
    /// because DataGrid cannot bind to ValueTuple fields.
    /// </summary>
    public class QueueItemDisplay
    {
        public int VisitId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public int TriageLevel { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;

        public QueueItemDisplay()
        {
        }

        public QueueItemDisplay(ER_Visit visit, Triage triage)
        {
            VisitId = visit.Visit_ID;
            PatientId = visit.Patient_ID;
            TriageLevel = triage.Triage_Level;
            Specialization = triage.Specialization;
            ArrivalTime = visit.Arrival_date_time;
            Status = visit.Status;
        }
    }
}
