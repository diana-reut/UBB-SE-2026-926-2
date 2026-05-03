using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IRegistrationService
    {
        ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint);
    }
}
