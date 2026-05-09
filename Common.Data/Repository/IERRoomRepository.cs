using Common.Data.Models;

namespace Common.Data.Repository;

public interface IERRoomRepository
{
    Task<List<ER_Room>> GetAllAsync();
    Task<ER_Room?> GetByIdAsync(int id);
    Task<ER_Room> CreateAsync(ER_Room room);
    Task<bool> UpdateAsync(int id, ER_Room room);
    Task<bool> DeleteAsync(int id);
}
