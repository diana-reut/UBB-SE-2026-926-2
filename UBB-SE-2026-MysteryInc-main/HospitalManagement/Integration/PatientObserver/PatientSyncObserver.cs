using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Proxy.PatientProxy;
using System;
using System.Linq;

namespace HospitalManagement.Integration.PatientObserver;

internal class PatientSyncObserver : IPatientObserver
{
    private readonly IPatientProxy _patientService;

    public PatientSyncObserver(IPatientProxy patientService)
    {
        _patientService = patientService;
    }

    public void OnNewExternalPatient(ExternalPatientDTO newPatientData)
    {
        if (newPatientData is null)
        {
            throw new ArgumentNullException(nameof(newPatientData), "Received null patient data from external provider.");
        }

        bool exists = _patientService.ExistsAsync(newPatientData.CNP).GetAwaiter().GetResult();

        if (exists)
        {
            Patient existing = _patientService.SearchPatientsAsync(new SearchPatientsDto { Cnp = newPatientData.CNP })
                .GetAwaiter().GetResult()
                .First();

            UpdatePatientDto updateDto = MapDTOToUpdateDto(newPatientData);
            _patientService.UpdatePatientAsync(existing.Id, updateDto).GetAwaiter().GetResult();
        }
        else
        {
            CreatePatientDto newPatient = MapDTOToCreateDto(newPatientData);
            _ = _patientService.CreatePatientAsync(newPatient).GetAwaiter().GetResult();
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
        };
    }

    private static CreatePatientDto MapDTOToCreateDto(ExternalPatientDTO dto)
    {
        return new CreatePatientDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Cnp = dto.CNP,
            Sex = dto.Sex,
        };
    }

    private static UpdatePatientDto MapDTOToUpdateDto(ExternalPatientDTO dto)
    {
        return new UpdatePatientDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Cnp = dto.CNP,
            Sex = dto.Sex,
        };
    }
}