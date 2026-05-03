using System.Collections.Generic;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Integration.PatientObserver;

namespace HospitalManagement.Integration.External;

internal class ExternalPatientPublisher : IExternalPatientPublisher
{
    private readonly List<IPatientObserver> _observers = [];

    public void Subscribe(IPatientObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(IPatientObserver observer)
    {
        _ = _observers.Remove(observer);
    }

    public void Notify(ExternalPatientDTO dto)
    {
        foreach (IPatientObserver observer in _observers)
        {
            observer.OnNewExternalPatient(dto);
        }
    }
}
