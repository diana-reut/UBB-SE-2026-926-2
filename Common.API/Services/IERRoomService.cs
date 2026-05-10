using Common.Data.Models;

namespace Common.API.Services;

public interface IERRoomService
{
    Task<List<ER_Room>> GetAllAsync();
    Task<ER_Room?> GetByIdAsync(int id);
    Task<ER_Room> CreateAsync(ER_Room room);
    Task<bool> UpdateAsync(int id, ER_Room room);
    Task<bool> DeleteAsync(int id);
}
