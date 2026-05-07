using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IPatientService
{
    Task ArchiveAsDeceasedAsync(int id, DateTime deathDate);
    Task ArchivePatientAsync(int id);
    Task CreateMedicalHistoryAsync(int patientId, MedicalHistory history);
    Task<Patient> CreatePatientAsync(Patient data);
    Task DearchivePatientAsync(int id);
    Task DeletePatientAsync(int id);
    Task<bool> ExistsAsync(string cnp);
    Task<Patient?> GetByIdAsync(int patientId);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    Task<List<string>> GetPatientAllergiesAsync(int patientId);
    Task<Patient> GetPatientDetailsAsync(int id);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId);
    Task<bool> IsHighRiskPatientAsync(int patientId);
    Task<List<Patient>> SearchPatientsAsync(PatientFilter filter);
    Task UpdatePatientAsync(Patient data);
    bool ValidateCNP(string cnp, Sex sex, DateTime dob);
}