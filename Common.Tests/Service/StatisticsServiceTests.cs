using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class StatisticsServiceTests
{
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IMedicalRecordRepository> _recordRepo = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepo = new();

    private StatisticsService CreateService()
    {
        return new StatisticsService(
            _patientRepo.Object,
            _recordRepo.Object,
            _prescriptionRepo.Object);
    }

    [TestMethod]
    public async Task GetPatientsByBloodTypeAsync_ReturnsPatientsGroupedByBloodType()
    {
        StatisticsService service = CreateService();

        BloodType bloodTypeA = (BloodType)0;
        BloodType bloodTypeB = (BloodType)1;

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                new Patient { MedicalHistory = new MedicalHistory { BloodType = bloodTypeA } },
                new Patient { MedicalHistory = new MedicalHistory { BloodType = bloodTypeA } },
                new Patient { MedicalHistory = new MedicalHistory { BloodType = bloodTypeB } },
                new Patient { MedicalHistory = new MedicalHistory { BloodType = null } },
                new Patient { MedicalHistory = null }
            });

        Dictionary<string, int> result = await service.GetPatientsByBloodTypeAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { bloodTypeA.ToString(), 2 },
                { bloodTypeB.ToString(), 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetPatientsByBloodTypeAsync_WhenNoPatients_ReturnsEmptyDictionary()
    {
        StatisticsService service = CreateService();

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>());

        Dictionary<string, int> result = await service.GetPatientsByBloodTypeAsync();

        AssertDictionariesAreEqual(new Dictionary<string, int>(), result);
    }

    [TestMethod]
    public async Task GetPatientsByRhAsync_ReturnsPatientsGroupedByRh()
    {
        StatisticsService service = CreateService();

        Rh positive = (Rh)0;
        Rh negative = (Rh)1;

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                new Patient { MedicalHistory = new MedicalHistory { Rh = positive } },
                new Patient { MedicalHistory = new MedicalHistory { Rh = positive } },
                new Patient { MedicalHistory = new MedicalHistory { Rh = negative } },
                new Patient { MedicalHistory = new MedicalHistory { Rh = null } },
                new Patient { MedicalHistory = null }
            });

        Dictionary<string, int> result = await service.GetPatientsByRhAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { positive.ToString(), 2 },
                { negative.ToString(), 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetPatientsByRhAsync_WhenNoValidRhValues_ReturnsEmptyDictionary()
    {
        StatisticsService service = CreateService();

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                new Patient { MedicalHistory = null },
                new Patient { MedicalHistory = new MedicalHistory { Rh = null } }
            });

        Dictionary<string, int> result = await service.GetPatientsByRhAsync();

        AssertDictionariesAreEqual(new Dictionary<string, int>(), result);
    }

    [TestMethod]
    public async Task GetPatientGenderDistributionAsync_ReturnsPatientsGroupedBySex()
    {
        StatisticsService service = CreateService();

        Sex firstSex = (Sex)0;
        Sex secondSex = (Sex)1;

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                new Patient { Sex = firstSex },
                new Patient { Sex = firstSex },
                new Patient { Sex = secondSex }
            });

        Dictionary<string, int> result = await service.GetPatientGenderDistributionAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { firstSex.ToString(), 2 },
                { secondSex.ToString(), 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetPatientGenderDistributionAsync_WhenNoPatients_ReturnsEmptyDictionary()
    {
        StatisticsService service = CreateService();

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>());

        Dictionary<string, int> result = await service.GetPatientGenderDistributionAsync();

        AssertDictionariesAreEqual(new Dictionary<string, int>(), result);
    }

    [TestMethod]
    public async Task GetConsultationDistributionAsync_ReturnsRecordsGroupedBySourceType()
    {
        StatisticsService service = CreateService();

        _recordRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<MedicalRecord>
            {
                new MedicalRecord { SourceType = SourceType.ER },
                new MedicalRecord { SourceType = SourceType.ER },
                new MedicalRecord { SourceType = SourceType.App }
            });

        Dictionary<string, int> result = await service.GetConsultationDistributionAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { SourceType.ER.ToString(), 2 },
                { SourceType.App.ToString(), 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetConsultationDistributionAsync_WhenNoRecords_ReturnsEmptyDictionary()
    {
        StatisticsService service = CreateService();

        _recordRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<MedicalRecord>());

        Dictionary<string, int> result = await service.GetConsultationDistributionAsync();

        AssertDictionariesAreEqual(new Dictionary<string, int>(), result);
    }

    [TestMethod]
    public async Task GetTopDiagnosesAsync_IgnoresEmptyDiagnosesAndGroupsCaseInsensitiveTrimmedValues()
    {
        StatisticsService service = CreateService();

        _recordRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<MedicalRecord>
            {
                new MedicalRecord { Diagnosis = " flu " },
                new MedicalRecord { Diagnosis = "FLU" },
                new MedicalRecord { Diagnosis = "Cold" },
                new MedicalRecord { Diagnosis = string.Empty },
                new MedicalRecord { Diagnosis = "   " },
                new MedicalRecord { Diagnosis = null }
            });

        Dictionary<string, int> result = await service.GetTopDiagnosesAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { "FLU", 2 },
                { "COLD", 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetTopDiagnosesAsync_WhenNoRecords_ReturnsEmptyDictionary()
    {
        StatisticsService service = CreateService();

        _recordRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<MedicalRecord>());

        Dictionary<string, int> result = await service.GetTopDiagnosesAsync();

        AssertDictionariesAreEqual(new Dictionary<string, int>(), result);
    }

    [TestMethod]
    public async Task GetMostPrescribedMedsAsync_IgnoresNullAndEmptyMedicationNamesAndGroupsCaseInsensitiveTrimmedValues()
    {
        StatisticsService service = CreateService();

        _prescriptionRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Prescription>
            {
                new Prescription
                {
                    MedicationList = new List<PrescriptionItem>
                    {
                        new PrescriptionItem { MedName = " aspirin " },
                        new PrescriptionItem { MedName = "ASPIRIN" },
                        new PrescriptionItem { MedName = "Ibuprofen" },
                        new PrescriptionItem { MedName = string.Empty },
                        new PrescriptionItem { MedName = "   " },
                        new PrescriptionItem { MedName = null }
                    }
                },
                new Prescription
                {
                    MedicationList = null
                }
            });

        Dictionary<string, int> result = await service.GetMostPrescribedMedsAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { "ASPIRIN", 2 },
                { "IBUPROFEN", 1 }
            },
            result);
    }

    [TestMethod]
    public async Task GetMostPrescribedMedsAsync_ReturnsOnlyTopTwentyMedications()
    {
        StatisticsService service = CreateService();

        List<PrescriptionItem> items = new();

        for (int i = 1; i <= 25; i++)
        {
            items.Add(new PrescriptionItem { MedName = $"Med{i}" });
        }

        _prescriptionRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Prescription>
            {
                new Prescription { MedicationList = items }
            });

        Dictionary<string, int> result = await service.GetMostPrescribedMedsAsync();

        Assert.AreEqual(20, result.Count);
    }

    [TestMethod]
    public async Task GetActiveVsArchivedRatioAsync_ReturnsActiveAndArchivedCounts()
    {
        StatisticsService service = CreateService();

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                new Patient { IsArchived = false },
                new Patient { IsArchived = false },
                new Patient { IsArchived = true }
            });

        Dictionary<string, int> result = await service.GetActiveVsArchivedRatioAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
                { "Active", 2 },
                { "Archived", 1 }
            },
            result);
    }

    private static void AssertDictionariesAreEqual(
        Dictionary<string, int> expected,
        Dictionary<string, int> actual)
    {
        CollectionAssert.AreEquivalent(
            expected.ToArray(),
            actual.ToArray());
    }

    [TestMethod]
    public async Task GetAgeDistributionAsync_WhenNoPatients_ReturnsZeroCountsForAllAgeGroups()
    {
        StatisticsService service = CreateService();

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>());

        Dictionary<string, int> result = await service.GetAgeDistributionAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
            { "Pediatric (0-17)", 0 },
            { "Adult (18-64)", 0 },
            { "Geriatric (65+)", 0 }
            },
            result);
    }

    [TestMethod]
    public async Task GetAgeDistributionAsync_ReturnsPatientsGroupedByAgeRange()
    {
        StatisticsService service = CreateService();

        DateTime today = DateTime.Today;

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
            new Patient { Date_of_Birth = today.AddYears(-17) },

            new Patient { Date_of_Birth = today.AddYears(-18) },

            new Patient { Date_of_Birth = today.AddYears(-64) },

            new Patient { Date_of_Birth = today.AddYears(-65) }
            });

        Dictionary<string, int> result = await service.GetAgeDistributionAsync();

        AssertDictionariesAreEqual(
            new Dictionary<string, int>
            {
            { "Pediatric (0-17)", 1 },
            { "Adult (18-64)", 2 },
            { "Geriatric (65+)", 1 }
            },
            result);
    }
}