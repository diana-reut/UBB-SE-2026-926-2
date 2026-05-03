using HospitalManagement.Entity.DTOs;

namespace HospitalManagement.Integration.External;

internal class StaffProxy : IExternalProvider  // THIS IS A SKELETON FOR THE NEXT TEAM , MEDICAL STAFF PART , IN THEIR TEMPORARY LIST LOGIC SHOULD IMPLEMENT THIS
{
    private readonly IExternalPatientPublisher _publisher;

    public StaffProxy(IExternalPatientPublisher publisher)
    {
        _publisher = publisher;
    }

    public ExternalPatientDTO FetchPatientById(int patientId)
    {
        throw new MyNotImplementedException("To be implemented by Staff team");
    }

    public RecordDTO FetchRecordByPatientId(int patientId)
    {
        throw new MyNotImplementedException("To be implemented by Staff team");
    }

    public void OnNewPatientDetected(ExternalPatientDTO dto)
    {
        _publisher.Notify(dto);
    }
}
