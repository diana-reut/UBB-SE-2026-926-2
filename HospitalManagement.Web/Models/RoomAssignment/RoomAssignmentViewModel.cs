namespace HospitalManagement.Web.Models.RoomAssignment;

public class RoomAssignmentViewModel
{
    public int? SelectedVisitId { get; set; }
    public int? SelectedRoomId { get; set; }
    public List<RoomAssignmentVisitViewModel> WaitingVisits { get; set; } = new ();
    public List<RoomOptionViewModel> AvailableRooms { get; set; } = new ();
    public RoomAssignmentPatientViewModel? SelectedPatient { get; set; }
    public RoomAssignmentTriageViewModel? SelectedTriage { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RoomAssignmentVisitViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? TriageLevel { get; set; }
    public string? Specialization { get; set; }
    public bool HasTriageData { get; set; }
    public string? WarningMessage { get; set; }
}

public class RoomOptionViewModel
{
    public int RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class RoomAssignmentPatientViewModel
{
    public string PatientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class RoomAssignmentTriageViewModel
{
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
}
