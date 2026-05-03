using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IPatientService
{
    public void ArchiveAsDeceased(int id, DateTime deathDate);

    public void ArchivePatient(int id);

    public void CreateMedicalHistory(int patientId, MedicalHistory history);

    public Patient CreatePatient(Patient data);

    public void DearchivePatient(int id);

    public void DeletePatient(int id);

    public bool Exists(string cnp);

    public MedicalHistory? GetMedicalHistory(int patientId);

    public List<MedicalRecord> GetMedicalRecords(int historyId);

    public List<string> GetPatientAllergies(int patientId);

    public Patient GetPatientDetails(int id);

    public Prescription? GetPrescriptionByRecordId(int recordId);

    public bool IsHighRiskPatient(int patientId);

    public List<Patient> SearchPatients(PatientFilter filter);

    public void UpdatePatient(Patient data);

    public bool ValidateCNP(string cnp, Sex sex, DateTime dob);

    public Patient GetById(int patientId);
}
