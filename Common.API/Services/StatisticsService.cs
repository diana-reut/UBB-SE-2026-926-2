using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Repository;
using Common.Data.Entity;

namespace Common.API.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IPatientRepository patientRepo;
    private readonly IMedicalRecordRepository recordRepo;
    private readonly IPrescriptionRepository prescriptionRepo;

    public StatisticsService(IPatientRepository patientRepo, IMedicalRecordRepository recordRepo, IPrescriptionRepository prescriptionRepo)
    {
        this.patientRepo = patientRepo;
        this.recordRepo = recordRepo;
        this.prescriptionRepo = prescriptionRepo;
    }

    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
    {
        return BuildPatientsByBloodType(await patientRepo.GetAllAsync(include_archived: true));
    }

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
    {
        return BuildPatientsByRh(await patientRepo.GetAllAsync(include_archived: true));
    }

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
    {
        return BuildPatientGenderDistribution(await patientRepo.GetAllAsync(include_archived: true));
    }

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
    {
        return BuildConsultationDistribution(await recordRepo.GetAllAsync());
    }

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
    {
        return BuildTopDiagnoses(await recordRepo.GetAllAsync());
    }

    public async Task<Dictionary<string, int>> GetAgeDistributionAsync()
    {
        return BuildAgeDistribution(await patientRepo.GetAllAsync(include_archived: true));
    }
    public async Task<Dictionary<string, int>> GetMostPrescribedMedsAsync()
    {
        return BuildMostPrescribedMeds(await prescriptionRepo.GetAllAsync());
    }

    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
    {
        return BuildActiveVsArchivedRatio(await patientRepo.GetAllAsync(include_archived: true));
    }

    private static Dictionary<string, int> BuildPatientsByBloodType(IEnumerable<Patient> patients)
    {
        return patients.Where(p => p.MedicalHistory?.BloodType.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.BloodType!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildPatientsByRh(IEnumerable<Patient> patients)
    {
        return patients.Where(p => p.MedicalHistory?.Rh.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.Rh!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildPatientGenderDistribution(IEnumerable<Patient> patients)
    {
        return patients.GroupBy(p => p.Sex.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildConsultationDistribution(IEnumerable<MedicalRecord> records)
    {
        return records
            .GroupBy(r => r.SourceType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildTopDiagnoses(IEnumerable<MedicalRecord> records)
    {
        return records.Where(r => !string.IsNullOrWhiteSpace(r.Diagnosis))
            .GroupBy(static r => r.Diagnosis!.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildAgeDistribution(IEnumerable<Patient> patients)
    {
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

    private static Dictionary<string, int> BuildMostPrescribedMeds(IEnumerable<Prescription> prescriptions)
    {
        IEnumerable<PrescriptionItem> allItems = prescriptions.Where(p => p.MedicationList is not null)
            .SelectMany(p => p.MedicationList);

        return allItems.Where(item => !string.IsNullOrWhiteSpace(item.MedName))
            .GroupBy(item => item.MedName.Trim().ToUpperInvariant())
            .OrderByDescending(g => g.Count())
            .Take(20)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildActiveVsArchivedRatio(IEnumerable<Patient> patients)
    {
        return new Dictionary<string, int>
        {
            { "Active", patients.Count(p => !p.IsArchived) },
            { "Archived", patients.Count(p => p.IsArchived) },
        };
    }
}
