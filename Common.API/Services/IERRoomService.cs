using Common.Data.Models;
using Common.Data.Entity.DTOs;

namespace Common.API.Services;

public interface IERRoomService
{
    Task<List<ER_Room>> GetAllAsync();
    Task<ER_Room?> GetByIdAsync(int id);
    Task<ER_Room> CreateAsync(ER_Room room);
    Task<bool> UpdateAsync(int id, ER_Room room);
    Task<bool> DeleteAsync(int id);
    Task<List<ER_Room>> GetByStatusAsync(string status);
    Task<ERRoomVisitDetailsDto?> GetVisitDetailsAsync(int roomId);
    Task MarkRoomAsCleaningAsync(int roomId);
    Task MarkRoomAsAvailableAsync(int roomId);
}
