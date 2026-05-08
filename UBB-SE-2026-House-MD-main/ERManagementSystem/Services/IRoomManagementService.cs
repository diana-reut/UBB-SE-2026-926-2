using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRoomManagementService
    {
        List<ER_Room> GetAvailableRooms();
        Task<List<ER_Room>> GetAvailableRoomsAsync();
        List<ER_Room> GetOccupiedRooms();
        Task<List<ER_Room>> GetOccupiedRoomsAsync();
        List<ER_Room> GetCleaningRooms();
        Task<List<ER_Room>> GetCleaningRoomsAsync();
        void MarkRoomAsCleaning(int roomId);
        Task MarkRoomAsCleaningAsync(int roomId);
        void MarkRoomAsCleaned(int roomId);
        Task MarkRoomAsCleanedAsync(int roomId);
        RoomVisitDetails? GetRoomVisitDetails(int roomId);
        Task<RoomVisitDetails?> GetRoomVisitDetailsAsync(int roomId);
    }
}
