namespace ERManagementSystem.Models
{
    public class TransferEligibleVisit
    {
        public int VisitId { get; set; }
        public string ChiefComplaint { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PatientFirstName { get; set; } = string.Empty;
        public string PatientLastName { get; set; } = string.Empty;
        public bool IsTransferred { get; set; }
    }
}
