using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.ERVisitProxy;

public interface IERVisitProxy
{
    Task<List<ER_Visit>> GetAllAsync();
    Task<ER_Visit?> GetByIdAsync(int id);
    Task<ER_Visit> CreateAsync(ER_Visit visit);
    Task UpdateAsync(int id, ER_Visit visit);
    Task DeleteAsync(int id);
    Task<List<ER_Visit>> GetByStatusAsync(string status);
    Task UpdateStatusAsync(int id, string status);
    Task<bool> AutoAssignHighestPriorityRoomAsync();
    Task AssignRoomAsync(int visitId, int roomId);
    Task TransferVisitAsync(int visitId);
    Task RetryTransferAsync(int visitId);
    Task CloseVisitAsync(int visitId);
}
