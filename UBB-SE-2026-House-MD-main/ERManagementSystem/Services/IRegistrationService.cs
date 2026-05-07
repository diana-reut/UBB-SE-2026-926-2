using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRegistrationService
    {
        ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint);
        Task<ER_Visit> RegisterPatientAndVisitAsync(Patient patient, string chiefComplaint);
    }
}
