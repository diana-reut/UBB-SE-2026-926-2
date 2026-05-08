using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Services
{
    public interface IQueueService
    {
        List<(ER_Visit visit, Triage triage)> GetOrderedQueue();
        Task<List<(ER_Visit visit, Triage triage)>> GetOrderedQueueAsync();
        void RemoveFromQueue(int visitId);
        Task RemoveFromQueueAsync(int visitId);
    }
}
