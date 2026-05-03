using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IQueueService
    {
        List<(ER_Visit visit, Triage triage)> GetOrderedQueue();
        void RemoveFromQueue(int visitId);
    }
}
