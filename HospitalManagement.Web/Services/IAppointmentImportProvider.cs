using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public interface IAppointmentImportProvider
{
    RecordDTO FetchRecordByPatientId(int patientId);
}
