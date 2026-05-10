using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Models;

namespace ERManagementSystem.Services
{
    public interface IRegistrationService
    {
        ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint);
        Task<ER_Visit> RegisterPatientAndVisitAsync(Patient patient, string chiefComplaint);
    }
}
