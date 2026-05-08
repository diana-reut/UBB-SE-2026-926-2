using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITriageParametersRepository
    {
        void Add(Triage_Parameters parameters);
        Task AddAsync(Triage_Parameters parameters);
        Triage_Parameters? GetByTriageId(int triageId);
        Task<Triage_Parameters?> GetByTriageIdAsync(int triageId);
        void Delete(Triage_Parameters parameters);
        Task DeleteAsync(Triage_Parameters parameters);
    }
}
