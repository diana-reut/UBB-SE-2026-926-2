using System;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Integration.External;

internal class MockERProxy : IExternalProvider
{
    private readonly IExternalPatientPublisher _publisher;

    public MockERProxy(IExternalPatientPublisher publisher)
    {
        _publisher = publisher;
    }

    public ExternalPatientDTO FetchPatientById(int patientId)
    {
        return new ExternalPatientDTO
        {
            CNP = "1990101123456",
            FirstName = "John",
            LastName = "Doe",
            Sex = Sex.M,
            EmergencyTimestamp = DateTime.Now,
            Injury = "Broken arm",
        };
    }

    public RecordDTO FetchRecordByPatientId(int patientId)
    {
        return new RecordDTO
        {
            ExternalRecordId = patientId,
            Symptoms = "Pain in left arm",
            TemporaryDiagnosis = "Suspected fracture",
            PrescribedMeds = "Ibuprofen 400mg",
            ConsultationDate = DateTime.Now,
            SourceType = SourceType.ER,
        };
    }

    public void OnNewPatientDetected(ExternalPatientDTO dto)
    {
        _publisher.Notify(dto);
    }
}
