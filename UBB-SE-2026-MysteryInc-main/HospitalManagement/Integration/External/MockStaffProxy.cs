using System;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Integration.External;

internal class MockStaffProxy : IExternalProvider
{
    private readonly IExternalPatientPublisher _publisher;

    public MockStaffProxy(IExternalPatientPublisher publisher)
    {
        _publisher = publisher;
    }

    public ExternalPatientDTO FetchPatientById(int patientId)
    {
        return new ExternalPatientDTO
        {
            CNP = "2850215654321",
            FirstName = "Jane",
            LastName = "Smith",
            Sex = Sex.F,
            EmergencyTimestamp = DateTime.Now,
            Injury = null,
        };
    }

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

    public void OnNewPatientDetected(ExternalPatientDTO dto)
    {
        _publisher.Notify(dto);
    }
}
