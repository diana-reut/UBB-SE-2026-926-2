using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IRoomRepository
    {
        List<ER_Room> GetAllRooms();
        ER_Room? GetById(int roomId);
        List<ER_Room> GetAvailableRooms();
        List<ER_Room> GetOccupiedRooms();
        List<ER_Room> GetCleaningRooms();
        List<ER_Room> GetRoomsByStatus(string status);
        void UpdateAvailabilityStatus(int roomId, string newStatus);
        void SetCurrentVisit(int roomId, int visitId);
        void ClearCurrentVisit(int roomId);
        int? GetRoomIdByVisitId(int visitId);
        int? GetRoomIdByCurrentVisit(int visitId);
        int? GetAssignedRoomIdForVisit(int visitId);
        ER_Visit? GetVisitByRoomId(int roomId);
    }
}
