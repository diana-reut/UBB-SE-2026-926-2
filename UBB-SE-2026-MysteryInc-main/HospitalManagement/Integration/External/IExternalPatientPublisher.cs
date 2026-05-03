using HospitalManagement.Entity.DTOs;
using HospitalManagement.Integration.PatientObserver;

namespace HospitalManagement.Integration.External;

internal interface IExternalPatientPublisher
{
    public void Notify(ExternalPatientDTO dto);

    public void Subscribe(IPatientObserver observer);

    public void Unsubscribe(IPatientObserver observer);
}
