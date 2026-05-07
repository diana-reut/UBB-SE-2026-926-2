using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IRoomRepository
    {
        List<ER_Room> GetAllRooms();
        Task<List<ER_Room>> GetAllRoomsAsync();
        ER_Room? GetById(int roomId);
        Task<ER_Room?> GetByIdAsync(int roomId);
        List<ER_Room> GetAvailableRooms();
        Task<List<ER_Room>> GetAvailableRoomsAsync();
        List<ER_Room> GetOccupiedRooms();
        Task<List<ER_Room>> GetOccupiedRoomsAsync();
        List<ER_Room> GetCleaningRooms();
        Task<List<ER_Room>> GetCleaningRoomsAsync();
        List<ER_Room> GetRoomsByStatus(string status);
        Task<List<ER_Room>> GetRoomsByStatusAsync(string status);
        void UpdateAvailabilityStatus(int roomId, string newStatus);
        Task UpdateAvailabilityStatusAsync(int roomId, string newStatus);
        void SetCurrentVisit(int roomId, int visitId);
        Task SetCurrentVisitAsync(int roomId, int visitId);
        void ClearCurrentVisit(int roomId);
        Task ClearCurrentVisitAsync(int roomId);
        int? GetRoomIdByVisitId(int visitId);
        Task<int?> GetRoomIdByVisitIdAsync(int visitId);
        int? GetRoomIdByCurrentVisit(int visitId);
        Task<int?> GetRoomIdByCurrentVisitAsync(int visitId);
        int? GetAssignedRoomIdForVisit(int visitId);
        Task<int?> GetAssignedRoomIdForVisitAsync(int visitId);
        ER_Visit? GetVisitByRoomId(int roomId);
        Task<ER_Visit?> GetVisitByRoomIdAsync(int roomId);
    }
}
