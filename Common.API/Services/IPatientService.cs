using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Entity;
using Common.Data.Integration;

namespace Common.API.Services;

public interface IPatientService
{
    bool ValidateCNP(string cnp, Sex sex, DateTime dob);
    Task<Patient> CreatePatientAsync(Patient data);
    Task UpdatePatientAsync(Patient data);
    Task ArchivePatientAsync(Patient patient);
    Task DearchivePatientAsync(int id);
    Task ArchiveAsDeceasedAsync(int id, DateTime deathDate);
    Task DeletePatientAsync(int id);
    Task<bool> ExistsAsync(string cnp);
    Task<Patient?> GetByIdAsync(int patientId);
    Task<Patient> GetPatientDetailsAsync(int id);
    Task<List<Patient>> SearchPatientsAsync(PatientFilter filter);
    Task<bool> IsHighRiskPatientAsync(int patientId);
    Task CreateMedicalHistoryAsync(int patientId, MedicalHistory history);
    Task<int> CreateMedicalRecordAsync(int patientId, MedicalRecord record);
    Task CreatePrescriptionAsync(int recordId, Prescription prescription);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId);
    Task<List<string>> GetPatientAllergiesAsync(int patientId);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId);
}
