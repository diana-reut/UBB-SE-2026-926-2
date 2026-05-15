using Common.Data.Models;

namespace Common.API.Services;

public interface IERVisitService
{
    Task<List<ER_Visit>> GetAllAsync();
    Task<ER_Visit?> GetByIdAsync(int id);
    Task<ER_Visit> CreateAsync(ER_Visit visit);
    Task<bool> UpdateAsync(int id, ER_Visit visit);
    Task<bool> DeleteAsync(int id);
    Task<bool> AutoAssignHighestPriorityRoomAsync();
    Task AssignRoomAsync(int visitId, int roomId);
    Task TransferVisitAsync(int visitId);
    Task RetryTransferAsync(int visitId);
    Task CloseVisitAsync(int visitId);
}
