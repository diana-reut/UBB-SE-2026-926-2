using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class BloodCompatibilityServiceTests
{
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IMedicalHistoryRepository> _historyRepo = new();

    private BloodCompatibilityService CreateService()
    {
        return new BloodCompatibilityService(
            _patientRepo.Object,
            _historyRepo.Object);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenRecipientDoesNotExist_ReturnsEmptyList()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Patient?)null);

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenRecipientHistoryDoesNotExist_ReturnsEmptyList()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Patient { Id = 1 });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync((MedicalHistory?)null);

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenRecipientBloodTypeIsNull_ReturnsEmptyList()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Patient { Id = 1 });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(new MedicalHistory
            {
                BloodType = null,
                Rh = Rh.Positive
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenRecipientRhIsNull_ReturnsEmptyList()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Patient { Id = 1 });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = null
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorIsRecipient_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(1)
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorHistoryIsNull_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync((MedicalHistory?)null);

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorBloodTypeIsNull_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(new MedicalHistory
            {
                BloodType = null,
                Rh = Rh.Positive
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorRhIsNull_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = null
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenBloodTypeIsIncompatible_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(CreateHistory(BloodType.B, Rh.Positive));

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenRhIsIncompatible_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Negative));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorHasAnaphylacticAllergy_SkipsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
            CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive,
                Allergies = new List<(Allergy Allergy, string SeverityLevel)>
                {
                (new Allergy { AllergyName = "Peanut" }, "anaphylactic")
                }
            });

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorHasNoAllergies_AddsDonor()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenDonorsHaveDifferentScores_ReturnsHighestScoreFirst()
    {
        BloodCompatibilityService service = CreateService();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1, new DateTime(1990, 1, 1), (Sex)0));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(new List<Patient>
            {
                CreatePatient(2, new DateTime(1970, 1, 1), (Sex)1),
                CreatePatient(3, new DateTime(1990, 1, 1), (Sex)0)
            });

        _historyRepo.Setup(x => x.GetByPatientIdAsync(2))
            .ReturnsAsync(CreateHistory(BloodType.O, Rh.Negative));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(3))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        CollectionAssert.AreEqual(
            new[] { 3, 2 },
            result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_WhenMoreThanTwentyCompatibleDonors_ReturnsTwenty()
    {
        BloodCompatibilityService service = CreateService();

        List<Patient> donors = Enumerable.Range(2, 25)
            .Select(id => CreatePatient(id))
            .ToList();

        _patientRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreatePatient(1));

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));

        _patientRepo.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(donors);

        foreach (Patient donor in donors)
        {
            _historyRepo.Setup(x => x.GetByPatientIdAsync(donor.Id))
                .ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
        }

        List<Patient> result = await service.GetTopCompatibleDonorsAsync(1);

        Assert.AreEqual(20, result.Count);
    }

    [TestMethod]
    public void CalculateScore_WhenDonorHistoryIsNull_ReturnsZero()
    {
        BloodCompatibilityService service = CreateService();

        Patient donor = CreatePatient(1);
        donor.MedicalHistory = null;

        Patient recipient = CreatePatient(2);
        recipient.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        int result = service.CalculateScore(donor, recipient);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CalculateScore_WhenRecipientHistoryIsNull_ReturnsZero()
    {
        BloodCompatibilityService service = CreateService();

        Patient donor = CreatePatient(1);
        donor.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        Patient recipient = CreatePatient(2);
        recipient.MedicalHistory = null;

        int result = service.CalculateScore(donor, recipient);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CalculateScore_WhenExactBloodRhMatchSameAgeSameSex_ReturnsOneHundred()
    {
        BloodCompatibilityService service = CreateService();

        Patient donor = CreatePatient(1, new DateTime(1990, 1, 1), (Sex)0);
        donor.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        Patient recipient = CreatePatient(2, new DateTime(1990, 1, 1), (Sex)0);
        recipient.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        int result = service.CalculateScore(donor, recipient);

        Assert.AreEqual(100, result);
    }

    [TestMethod]
    public void CalculateScore_WhenBloodRhNotExactAndSexDifferent_ReturnsLowerScore()
    {
        BloodCompatibilityService service = CreateService();

        Patient donor = CreatePatient(1, new DateTime(1980, 1, 1), (Sex)0);
        donor.MedicalHistory = CreateHistory(BloodType.O, Rh.Negative);

        Patient recipient = CreatePatient(2, new DateTime(1990, 1, 1), (Sex)1);
        recipient.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        int result = service.CalculateScore(donor, recipient);

        Assert.AreEqual(55, result);
    }

    [TestMethod]
    public void CalculateScore_WhenAgeGapIsLarge_DoesNotAddNegativeAgePoints()
    {
        BloodCompatibilityService service = CreateService();

        Patient donor = CreatePatient(1, new DateTime(1950, 1, 1), (Sex)0);
        donor.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        Patient recipient = CreatePatient(2, new DateTime(1990, 1, 1), (Sex)0);
        recipient.MedicalHistory = CreateHistory(BloodType.A, Rh.Positive);

        int result = service.CalculateScore(donor, recipient);

        Assert.AreEqual(70, result);
    }

    [DataTestMethod]
    [DataRow(BloodType.O, BloodType.O, true)]
    [DataRow(BloodType.O, BloodType.A, true)]
    [DataRow(BloodType.O, BloodType.B, true)]
    [DataRow(BloodType.O, BloodType.AB, true)]
    [DataRow(BloodType.A, BloodType.A, true)]
    [DataRow(BloodType.A, BloodType.AB, true)]
    [DataRow(BloodType.A, BloodType.B, false)]
    [DataRow(BloodType.A, BloodType.O, false)]
    [DataRow(BloodType.B, BloodType.B, true)]
    [DataRow(BloodType.B, BloodType.AB, true)]
    [DataRow(BloodType.B, BloodType.A, false)]
    [DataRow(BloodType.B, BloodType.O, false)]
    [DataRow(BloodType.AB, BloodType.AB, true)]
    [DataRow(BloodType.AB, BloodType.A, false)]
    [DataRow(BloodType.AB, BloodType.B, false)]
    [DataRow(BloodType.AB, BloodType.O, false)]
    public void IsBloodMatch_ReturnsExpectedResult(
        BloodType donor,
        BloodType receiver,
        bool expected)
    {
        BloodCompatibilityService service = CreateService();

        bool result = service.IsBloodMatch(donor, receiver);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void IsBloodMatch_WhenDonorIsNull_ReturnsFalse()
    {
        BloodCompatibilityService service = CreateService();

        bool result = service.IsBloodMatch(null, BloodType.A);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsBloodMatch_WhenDonorEnumValueIsUnknown_ReturnsFalse()
    {
        BloodCompatibilityService service = CreateService();

        bool result = service.IsBloodMatch((BloodType)999, BloodType.A);

        Assert.IsFalse(result);
    }

    [DataTestMethod]
    [DataRow(Rh.Positive, Rh.Positive, true)]
    [DataRow(Rh.Negative, Rh.Positive, true)]
    [DataRow(Rh.Positive, Rh.Negative, false)]
    [DataRow(Rh.Negative, Rh.Negative, true)]
    public void IsRhMatch_ReturnsExpectedResult(
        Rh donor,
        Rh receiver,
        bool expected)
    {
        BloodCompatibilityService service = CreateService();

        bool result = service.IsRhMatch(donor, receiver);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void IsRhMatch_WhenDonorIsNull_ReturnsFalse()
    {
        BloodCompatibilityService service = CreateService();

        bool result = service.IsRhMatch(null, Rh.Positive);

        Assert.IsFalse(result);
    }

    private static Patient CreatePatient(
    int id,
    DateTime? dob = null,
    Sex sex = default)
    {
        return new Patient
        {
            Id = id,
            Dob = dob ?? new DateTime(1990, 1, 1),
            Sex = sex
        };
    }

    private static MedicalHistory CreateHistory(
        BloodType? bloodType,
        Rh? rh)
    {
        return new MedicalHistory
        {
            BloodType = bloodType,
            Rh = rh
        };
    }
}