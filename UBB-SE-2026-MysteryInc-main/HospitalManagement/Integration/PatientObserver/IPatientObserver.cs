using Common.Data.Entity.DTOs;

namespace HospitalManagement.Integration.PatientObserver;

internal interface IPatientObserver
{
    public void OnNewExternalPatient(ExternalPatientDTO newPatientData);
}
