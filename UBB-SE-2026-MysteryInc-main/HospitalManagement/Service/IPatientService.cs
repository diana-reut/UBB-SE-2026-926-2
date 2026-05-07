using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IPatientService
{
    void ArchiveAsDeceased(int id, DateTime deathDate);
    Task ArchiveAsDeceasedAsync(int id, DateTime deathDate);
    void ArchivePatient(int id);
    Task ArchivePatientAsync(Patient patient);
    void CreateMedicalHistory(int patientId, MedicalHistory history);
    Task CreateMedicalHistoryAsync(int patientId, MedicalHistory history);
    Patient CreatePatient(Patient data);
    Task<Patient> CreatePatientAsync(Patient data);
    void DearchivePatient(int id);
    Task DearchivePatientAsync(int id);
    void DeletePatient(int id);
    Task DeletePatientAsync(int id);
    bool Exists(string cnp);
    Task<bool> ExistsAsync(string cnp);
    Patient? GetById(int patientId);
    Task<Patient?> GetByIdAsync(int patientId);
    MedicalHistory? GetMedicalHistory(int patientId);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId);
    List<MedicalRecord> GetMedicalRecords(int historyId);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    List<string> GetPatientAllergies(int patientId);
    Task<List<string>> GetPatientAllergiesAsync(int patientId);
    Patient GetPatientDetails(int id);
    Task<Patient> GetPatientDetailsAsync(int id);
    Prescription? GetPrescriptionByRecordId(int recordId);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId);
    bool IsHighRiskPatient(int patientId);
    Task<bool> IsHighRiskPatientAsync(int patientId);
    List<Patient> SearchPatients(PatientFilter filter);
    Task<List<Patient>> SearchPatientsAsync(PatientFilter filter);
    Task UpdatePatientAsync(Patient data);
    bool ValidateCNP(string cnp, Sex sex, DateTime dob);
}
