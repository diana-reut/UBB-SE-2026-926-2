using Common.Data.Entity;
using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRegistrationService
    {
        ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint);
    }
}
