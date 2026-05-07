using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IImportService
{
    public void ImportFromAppointment(int patientId, int externalId);
    public Task ImportFromAppointmentAsync(int patientId, int externalId);

    public void ImportFromER(int patientId, int externalId);
    public Task ImportFromERAsync(int patientId, int externalId);
}
