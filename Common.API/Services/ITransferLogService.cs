using Common.Data.Models;
using Common.Data.Entity.DTOs;

namespace Common.API.Services;

public interface ITransferLogService
{
    Task<List<Transfer_Log>> GetAllAsync();
    Task<Transfer_Log?> GetByIdAsync(int id);
    Task<Transfer_Log> CreateAsync(Transfer_Log transferLog);
    Task<bool> UpdateAsync(int id, Transfer_Log transferLog);
    Task<bool> DeleteAsync(int id);
    Task<List<Transfer_Log>> GetByVisitIdAsync(int visitId);
    Task<List<ERTransferEligibleVisitDto>> GetEligibleVisitsAsync();
}
