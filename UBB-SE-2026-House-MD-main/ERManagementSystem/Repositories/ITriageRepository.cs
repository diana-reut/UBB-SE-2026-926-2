using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITriageRepository
    {
        int Add(Triage triage);
        Task<int> AddAsync(Triage triage);
        Triage? GetByVisitId(int visitId);
        Task<Triage?> GetByVisitIdAsync(int visitId);
        void Delete(Triage triage);
        Task DeleteAsync(Triage triage);
    }
}
