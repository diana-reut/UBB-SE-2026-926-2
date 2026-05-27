using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Services;

public class MockAppointmentImportProvider : IAppointmentImportProvider
{
    public RecordDTO FetchRecordByPatientId(int patientId)
    {
        return new RecordDTO
        {
            ExternalRecordId = patientId,
            Symptoms = "Persistent headache",
            TemporaryDiagnosis = "Migraine",
            PrescribedMeds = "Sumatriptan 50mg",
            ConsultationDate = DateTime.Now,
            SourceType = SourceType.App,
        };
    }
}
