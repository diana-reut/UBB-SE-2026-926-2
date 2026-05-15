using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.ERRoomProxy;

public interface IERRoomProxy
{
    Task<List<ER_Room>> GetAllAsync();
    Task<ER_Room?> GetByIdAsync(int id);
    Task<ER_Room> CreateAsync(ER_Room room);
    Task UpdateAsync(int id, ER_Room room);
    Task DeleteAsync(int id);
    Task<List<ER_Room>> GetRoomsByStatusAsync(string status);
    Task<List<ER_Room>> GetAvailableRoomsAsync();
    Task<List<ER_Room>> GetOccupiedRoomsAsync();
    Task<List<ER_Room>> GetCleaningRoomsAsync();
    Task SetCurrentVisitAsync(int roomId, int visitId);
    Task ClearCurrentVisitAsync(int roomId);
    Task<ERRoomVisitDetailsDto?> GetVisitDetailsAsync(int roomId);
    Task MarkRoomAsCleaningAsync(int roomId);
    Task MarkRoomAsAvailableAsync(int roomId);
}
