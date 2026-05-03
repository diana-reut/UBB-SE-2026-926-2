using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRoomManagementService
    {
        List<ER_Room> GetAvailableRooms();
        List<ER_Room> GetOccupiedRooms();
        List<ER_Room> GetCleaningRooms();
        void MarkRoomAsCleaning(int roomId);
        void MarkRoomAsCleaned(int roomId);
        RoomVisitDetails? GetRoomVisitDetails(int roomId);
    }
}
