namespace ERManagementSystem.Models
{
    public class RoomVisitDetails
    {
        public ER_Visit? Visit { get; set; }
        public Patient? Patient { get; set; }
        public Triage? Triage { get; set; }
    }
}
