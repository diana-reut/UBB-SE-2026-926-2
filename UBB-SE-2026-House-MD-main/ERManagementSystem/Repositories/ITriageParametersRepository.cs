using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITriageParametersRepository
    {
        void Add(Triage_Parameters parameters);
        Triage_Parameters? GetByTriageId(int triageId);
        void Delete(Triage_Parameters parameters);
    }
}
