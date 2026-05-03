using HospitalManagement.Entity.DTOs;

namespace HospitalManagement.Integration.External;

internal class ERProxy : IExternalProvider // THIS IS A SKELETON FOR THE NEXT TEAM , ER PART , IN THEIR TEMPORARY LIST LOGIC SHOULD IMPLEMENT THIS
{
    private readonly IExternalPatientPublisher _publisher;

    public ERProxy(IExternalPatientPublisher publisher)
    {
        _publisher = publisher;
    }

    public ExternalPatientDTO FetchPatientById(int patientId)
    {
        throw new MyNotImplementedException("To be implemented by ER team");
    }

    public RecordDTO FetchRecordByPatientId(int patientId)
    {
        throw new MyNotImplementedException("To be implemented by ER team");
    }

    public void OnNewPatientDetected(ExternalPatientDTO dto)
    {
        _publisher.Notify(dto);
    }
}
