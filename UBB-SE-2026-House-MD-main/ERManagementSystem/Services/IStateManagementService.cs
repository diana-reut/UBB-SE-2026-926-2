using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IStateManagementService
    {
        bool CanTransitionTo(string currentStatus, string newStatus);
        void ChangeStatus(ER_Visit visit, string newStatus);
        bool ValidateTransition(string currentStatus, string newStatus);
        void ChangeVisitStatus(int visitId, string newStatus);
        Task ChangeVisitStatusAsync(int visitId, string newStatus);
        bool CanClose(ER_Visit visit);
        void CloseVisit(int visitId);
        Task CloseVisitAsync(int visitId);
        List<ER_Visit> GetByStatus(string status);
        Task<List<ER_Visit>> GetByStatusAsync(string status);
    }
}
