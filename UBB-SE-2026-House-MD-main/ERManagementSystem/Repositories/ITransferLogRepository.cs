using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITransferLogRepository
    {
        void Add(Transfer_Log log);
        List<Transfer_Log> GetByVisitId(int visitId);
        List<Transfer_Log> GetAll();
        void DeleteLog(Transfer_Log log);
        void UpdateStatus(int transferId, string newStatus);
    }
}
