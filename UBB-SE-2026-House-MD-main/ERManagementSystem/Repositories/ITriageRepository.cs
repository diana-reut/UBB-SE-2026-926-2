using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface ITriageRepository
    {
        int Add(Triage triage);
        Triage? GetByVisitId(int visitId);
        void Delete(Triage triage);
    }
}
