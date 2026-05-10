using Common.Data.Models;

namespace Common.Data.Repository;

public interface ITransferLogRepository
{
    Task<List<Transfer_Log>> GetAllAsync();
    Task<Transfer_Log?> GetByIdAsync(int id);
    Task<Transfer_Log> CreateAsync(Transfer_Log transferLog);
    Task<bool> UpdateAsync(int id, Transfer_Log transferLog);
    Task<bool> DeleteAsync(int id);
}
