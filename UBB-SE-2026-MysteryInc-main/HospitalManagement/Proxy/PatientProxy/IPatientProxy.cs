using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Common.Data.Entity.DTOs;
using Common.Data.Entity;
using Common.Data.Integration;

namespace HospitalManagement.Proxy.PatientProxy;

internal interface IPatientProxy
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient> GetPatientDetailsAsync(int id);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int id);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    Task<int> CreateMedicalRecordAsync(int patientId, CreateMedicalRecordDto dto);
    Task CreatePrescriptionForRecordAsync(int recordId, CreatePrescriptionDto dto);
    Task<List<string>> GetPatientAllergiesAsync(int id);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId);
    Task<bool> IsHighRiskPatientAsync(int id);
    Task<bool> ExistsAsync(string cnp);
    Task<List<Patient>> SearchPatientsAsync(SearchPatientsDto dto);
    Task<Patient> CreatePatientAsync(CreatePatientDto dto);
    Task UpdatePatientAsync(int id, UpdatePatientDto dto);
    Task ArchivePatientAsync(int id);
    Task DearchivePatientAsync(int id);
    Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedDto dto);
    Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryDto dto);
    Task DeletePatientAsync(int id);
}
