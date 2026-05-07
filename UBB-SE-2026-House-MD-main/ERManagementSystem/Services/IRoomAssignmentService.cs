using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRoomAssignmentService
    {
        ER_Room? FindAvailableRoom(string requiredRoomType);
        Task<ER_Room?> FindAvailableRoomAsync(string requiredRoomType);
        void AssignRoomToVisit(int visitId, int roomId);
        Task AssignRoomToVisitAsync(int visitId, int roomId);
        void UpdateRoomAvailability(int roomId, string newStatus);
        Task UpdateRoomAvailabilityAsync(int roomId, string newStatus);
        bool AutoAssignRoom();
        Task<bool> AutoAssignRoomAsync();
        IReadOnlyList<(ER_Visit visit, Triage triage)> GetWaitingVisitsWithTriage();
        Task<IReadOnlyList<(ER_Visit visit, Triage triage)>> GetWaitingVisitsWithTriageAsync();
        IReadOnlyList<ER_Room> GetAvailableRooms();
        Task<IReadOnlyList<ER_Room>> GetAvailableRoomsAsync();
        Patient? GetPatientById(string patientId);
        Task<Patient?> GetPatientByIdAsync(string patientId);
        Triage? GetTriageByVisitId(int visitId);
        Task<Triage?> GetTriageByVisitIdAsync(int visitId);
    }
}
