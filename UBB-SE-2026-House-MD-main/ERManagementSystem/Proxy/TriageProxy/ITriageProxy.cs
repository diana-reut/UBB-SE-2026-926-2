using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.TriageProxy;

public interface ITriageProxy
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<Triage> CreateTriageAsync(int visitId, Triage_Parameters parameters);
    Task UpdateAsync(int id, Triage triage);
    Task DeleteAsync(int id);
    Task<Triage?> GetByVisitIdAsync(int visitId);
    Task<IReadOnlyList<ER_Visit>> GetVisitsForTriageAsync();
    Task MoveVisitToQueueAsync(int visitId);
    Task CloseVisitAsync(int visitId);
}
