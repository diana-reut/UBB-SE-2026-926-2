using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface ITriageService
    {
        Triage CreateTriage(int visitId, Triage_Parameters parameters);
        Task<Triage> CreateTriageAsync(int visitId, Triage_Parameters parameters);
        Triage? GetByVisitId(int visitId);
        Task<Triage?> GetByVisitIdAsync(int visitId);
        IReadOnlyList<ER_Visit> GetVisitsForTriage();
        Task<IReadOnlyList<ER_Visit>> GetVisitsForTriageAsync();
        void MoveVisitToQueue(int visitId);
        Task MoveVisitToQueueAsync(int visitId);
        void CloseVisit(int visitId);
        Task CloseVisitAsync(int visitId);
    }
}
