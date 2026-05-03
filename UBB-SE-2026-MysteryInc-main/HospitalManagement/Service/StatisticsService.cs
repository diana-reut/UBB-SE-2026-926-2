using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Entity;
using HospitalManagement.Repository;

namespace HospitalManagement.Service;

internal class StatisticsService : IStatisticsService
{
    private readonly IPatientRepository _patientRepo;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository _prescriptionRepo;

    public StatisticsService(IPatientRepository patientRepo, IMedicalRecordRepository recordRepo, IPrescriptionRepository prescriptionRepo)
    {
        _patientRepo = patientRepo;
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
    }

    public Dictionary<string, int> GetPatientsByBloodType()
    {
        IEnumerable<Patient> patients = _patientRepo.GetAll(true);

        var bloodCount = patients.Where(p => p.MedicalHistory?.BloodType.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.BloodType!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return bloodCount;
    }

    public Dictionary<string, int> GetPatientsByRh()
    {
        IEnumerable<Patient> patients = _patientRepo.GetAll(true);

        var rhCount = patients.Where(p => p.MedicalHistory?.Rh.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.Rh!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return rhCount;
    }

    public Dictionary<string, int> GetPatientGenderDistribution()
    {
        IEnumerable<Patient> patients = _patientRepo.GetAll(true);

        var genderCount = patients.GroupBy(p => p.Sex.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return genderCount;
    }

    public Dictionary<string, int> GetConsultationDistribution()
    {
        IEnumerable<MedicalRecord> records = _recordRepo.GetAll();

        var consultationTypeCount = records.GroupBy(r => r.SourceType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return consultationTypeCount;
    }

    public Dictionary<string, int> GetTopDiagnoses()
    {
        IEnumerable<MedicalRecord> records = _recordRepo.GetAll();

        var diagnosesCount = records.Where(r => !string.IsNullOrWhiteSpace(r.Diagnosis))
            .GroupBy(static r => r.Diagnosis!.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.Count());

        return diagnosesCount;
    }

    public Dictionary<string, int> GetAgeDistribution()
    {
        IEnumerable<Patient> patients = _patientRepo.GetAll(true);

        var ageGroups = new Dictionary<string, int>
            {
                { "Pediatric (0-17)", 0 },
                { "Adult (18-64)", 0 },
                { "Geriatric (65+)", 0 },
            };

        foreach (Patient patient in patients)
        {
            int age = patient.GetAge();

            if (age <= 17)
            {
                ageGroups["Pediatric (0-17)"]++;
            }
            else if (age <= 64)
            {
                ageGroups["Adult (18-64)"]++;
            }
            else
            {
                ageGroups["Geriatric (65+)"]++;
            }
        }

        return ageGroups;
    }

    public Dictionary<string, int> GetMostPrescribedMeds()
    {
        IEnumerable<Prescription> prescriptions = _prescriptionRepo.GetAll();

        IEnumerable<PrescriptionItem> allItems = prescriptions.Where(p => p.MedicationList is not null)
            .SelectMany(p => p.MedicationList);

        var topMeds = allItems.Where(item => !string.IsNullOrWhiteSpace(item.MedName) && !string.IsNullOrEmpty(item.MedName))
            .GroupBy(item => item.MedName.Trim().ToUpperInvariant())
            .OrderByDescending(g => g.Count())
            .Take(20)
            .ToDictionary(g => g.Key, g => g.Count());

        return topMeds;
    }

    public Dictionary<string, int> GetActiveVsArchivedRatio()
    {
        IEnumerable<Patient> patients = _patientRepo.GetAll(true);

        return new Dictionary<string, int>
        {
            { "Active", patients.Count(p => !p.IsArchived) },
            { "Archived", patients.Count(p => p.IsArchived) },
        };
    }
}
