namespace HospitalManagement.Web.Models.RoomManagement;

public class RoomManagementViewModel
{
    public List<RoomStatusItemViewModel> AvailableRooms { get; set; } = new ();
    public List<RoomStatusItemViewModel> OccupiedRooms { get; set; } = new ();
    public List<RoomStatusItemViewModel> CleaningRooms { get; set; } = new ();
    public int TotalRooms => AvailableRooms.Count + OccupiedRooms.Count + CleaningRooms.Count;
    public int? SelectedRoomId { get; set; }
    public RoomVisitDetailsViewModel? SelectedRoomVisit { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RoomStatusItemViewModel
{
    public int RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? CurrentVisitId { get; set; }
}

public class RoomVisitDetailsViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string VisitStatus { get; set; } = string.Empty;
    public int? TriageLevel { get; set; }
    public string? Specialization { get; set; }
}
