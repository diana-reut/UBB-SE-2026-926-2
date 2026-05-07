using System.Collections.Generic;
using Common.Data.Entity;
using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRoomAssignmentService
    {
        ER_Room? FindAvailableRoom(string requiredRoomType);
        void AssignRoomToVisit(int visitId, int roomId);
        void UpdateRoomAvailability(int roomId, string newStatus);
        bool AutoAssignRoom();
        IReadOnlyList<(ER_Visit visit, Triage triage)> GetWaitingVisitsWithTriage();
        IReadOnlyList<ER_Room> GetAvailableRooms();
        Patient? GetPatientById(string patientId);
        Triage? GetTriageByVisitId(int visitId);
    }
}
