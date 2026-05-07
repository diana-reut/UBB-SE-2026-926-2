using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IERVisitRepository
    {
        void Add(ER_Visit visit);
        Task AddAsync(ER_Visit visit);
        List<ER_Visit> GetActiveVisits();
        Task<List<ER_Visit>> GetActiveVisitsAsync();
        void UpdateStatus(int visitId, string newStatus);
        Task UpdateStatusAsync(int visitId, string newStatus);
        ER_Visit? GetByVisitId(int visitId);
        Task<ER_Visit?> GetByVisitIdAsync(int visitId);
        List<ER_Visit> GetByStatus(string status);
        Task<List<ER_Visit>> GetByStatusAsync(string status);
        List<(ER_Visit visit, Triage triage)> GetActiveVisitsWithTriage();
        Task<List<(ER_Visit visit, Triage triage)>> GetActiveVisitsWithTriageAsync();
    }
}
