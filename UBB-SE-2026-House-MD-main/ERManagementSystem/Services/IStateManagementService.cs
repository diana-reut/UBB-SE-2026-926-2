using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IStateManagementService
    {
        bool CanTransitionTo(string currentStatus, string newStatus);
        void ChangeStatus(ER_Visit visit, string newStatus);
        bool ValidateTransition(string currentStatus, string newStatus);
        void ChangeVisitStatus(int visitId, string newStatus);
        bool CanClose(ER_Visit visit);
        void CloseVisit(int visitId);
        List<ER_Visit> GetByStatus(string status);
    }
}
