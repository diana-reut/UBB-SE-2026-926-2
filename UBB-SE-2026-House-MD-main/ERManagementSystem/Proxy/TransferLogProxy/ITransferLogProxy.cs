using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.TransferLogProxy;

public interface ITransferLogProxy
{
    Task<List<Transfer_Log>> GetAllAsync();
    Task<Transfer_Log?> GetByIdAsync(int id);
    Task<Transfer_Log> CreateAsync(Transfer_Log transferLog);
    Task UpdateAsync(int id, Transfer_Log transferLog);
    Task DeleteAsync(int id);
    Task<List<Transfer_Log>> GetByVisitIdAsync(int visitId);
    Task<List<ERTransferEligibleVisitDto>> GetEligibleVisitsAsync();
}
