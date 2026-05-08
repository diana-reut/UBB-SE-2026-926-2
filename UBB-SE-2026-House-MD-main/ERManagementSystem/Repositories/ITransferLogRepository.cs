using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITransferLogRepository
    {
        void Add(Transfer_Log log);
        Task AddAsync(Transfer_Log log);
        List<Transfer_Log> GetByVisitId(int visitId);
        Task<List<Transfer_Log>> GetByVisitIdAsync(int visitId);
        List<Transfer_Log> GetAll();
        Task<List<Transfer_Log>> GetAllAsync();
        void DeleteLog(Transfer_Log log);
        Task DeleteLogAsync(Transfer_Log log);
        void UpdateStatus(int transferId, string newStatus);
        Task UpdateStatusAsync(int transferId, string newStatus);
    }
}
