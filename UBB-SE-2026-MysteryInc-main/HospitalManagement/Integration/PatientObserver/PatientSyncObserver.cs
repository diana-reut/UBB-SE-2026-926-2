using HospitalManagement.Entity.DTOs;
using HospitalManagement.Entity;
using HospitalManagement.Service;
using System;

namespace HospitalManagement.Integration.PatientObserver;

internal class PatientSyncObserver : IPatientObserver
{
    private readonly IPatientService _patientService;

    public PatientSyncObserver(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public void OnNewExternalPatient(ExternalPatientDTO newPatientData)
    {
        if (newPatientData is null)
        {
            throw new ArgumentNullException(nameof(newPatientData), "Received null patient data from external provider.");
        }

        // IN6: check if patient exists by CNP
        bool exists = _patientService.Exists(newPatientData.CNP);

        if (exists)
        {
            // map DTO to patient and update
            Patient updated = MapDTOToPatient(newPatientData);
            _patientService.UpdatePatient(updated);
        }
        else
        {
            // map DTO to new patient and create
            Patient newPatient = MapDTOToPatient(newPatientData);
            _ = _patientService.CreatePatient(newPatient);
        }
    }

    private static Patient MapDTOToPatient(ExternalPatientDTO dto)
    {
        return new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Cnp = dto.CNP,
            Sex = dto.Sex,
            // Injury -> goes to MedicalRecord.Symptoms, not Patient
            // EmergencyTimestamp -> goes to MedicalRecord.ConsultationDate
        };
    }
}
