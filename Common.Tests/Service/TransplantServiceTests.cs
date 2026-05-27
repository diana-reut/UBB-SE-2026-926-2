using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class TransplantServiceTests
{
    private Mock<ITransplantRepository> _transplantRepository = null!;
    private Mock<IPatientRepository> _patientRepository = null!;
    private Mock<IMedicalRecordRepository> _recordRepository = null!;
    private Mock<IBloodCompatibilityService> _compatibilityService = null!;
    private Mock<IMedicalHistoryRepository> _historyRepository = null!;
    private TransplantService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _transplantRepository = new Mock<ITransplantRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _recordRepository = new Mock<IMedicalRecordRepository>();
        _compatibilityService = new Mock<IBloodCompatibilityService>();
        _historyRepository = new Mock<IMedicalHistoryRepository>();
        _sut = new TransplantService(
            _transplantRepository.Object,
            _patientRepository.Object,
            _recordRepository.Object,
            _compatibilityService.Object,
            _historyRepository.Object);
    }

    private static Patient MakePatient(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Doe",
        Cnp = "1234567890123",
        PhoneNo = "0700",
        EmergencyContact = "John",
        Dob = new DateTime(1990, 1, 1),
        Sex = Sex.F
    };

    private static Patient MakeDeceasedDonor(int id = 1) => new()
    {
        Id = id,
        FirstName = "Don",
        LastName = "Or",
        Cnp = "1234567890123",
        PhoneNo = "0700",
        EmergencyContact = "John",
        Dob = new DateTime(1980, 1, 1),
        Sex = Sex.M,
        Dod = new DateTime(2026, 1, 1),
        IsDonor = true
    };

    private static MedicalHistory MakeHistory(BloodType? bloodType = BloodType.O, Rh? rh = Rh.Positive) => new()
    {
        BloodType = bloodType,
        Rh = rh,
        ChronicConditions = new List<string>()
    };

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_WhenReceiverDoesNotExist_ThrowsArgumentException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateWaitlistRequestAsync(7, "Kidney"));
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _transplantRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Transplant { TransplantId = 1 },
            new Transplant { TransplantId = 2 }
        ]);

        List<Transplant> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        Transplant transplant = new() { TransplantId = 3 };
        _transplantRepository.Setup(x => x.AddAsync(transplant)).Returns(Task.CompletedTask);

        await _sut.CreateAsync(transplant);

        _transplantRepository.Verify(x => x.AddAsync(transplant), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        Transplant transplant = new() { TransplantId = 3 };
        _transplantRepository.Setup(x => x.UpdateAsync(3, transplant)).ReturnsAsync(true);

        await _sut.UpdateAsync(3, transplant);

        _transplantRepository.Verify(x => x.UpdateAsync(3, transplant), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _transplantRepository.Setup(x => x.DeleteAsync(3)).ReturnsAsync(true);

        await _sut.DeleteAsync(3);

        _transplantRepository.Verify(x => x.DeleteAsync(3), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsTransplant_ReturnsSameInstance()
    {
        Transplant transplant = new() { TransplantId = 3 };
        _transplantRepository.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(transplant);

        Transplant? result = await _sut.GetByIdAsync(3);

        Assert.AreSame(transplant, result);
    }

    [TestMethod]
    public async Task GetByReceiverIdAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _transplantRepository.Setup(x => x.GetByReceiverIdAsync(5)).ReturnsAsync([
            new Transplant { TransplantId = 1 },
            new Transplant { TransplantId = 2 }
        ]);

        List<Transplant> result = await _sut.GetByReceiverIdAsync(5);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByDonorIdAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _transplantRepository.Setup(x => x.GetByDonorIdAsync(5)).ReturnsAsync([
            new Transplant { TransplantId = 1 },
            new Transplant { TransplantId = 2 }
        ]);

        List<Transplant> result = await _sut.GetByDonorIdAsync(5);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task AssignDonorAsync_WhenCalled_DelegatesToRepository()
    {
        _transplantRepository.Setup(x => x.UpdateAsync(3, 5, 88)).Returns(Task.CompletedTask);

        await _sut.AssignDonorAsync(3, 5, 88);

        _transplantRepository.Verify(x => x.UpdateAsync(3, 5, 88), Times.Once);
    }

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_WhenOrganTypeIsLungs_NormalizesToLung()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(MakePatient(7));
        _transplantRepository.Setup(x => x.AddAsync(It.IsAny<Transplant>())).Returns(Task.CompletedTask);

        await _sut.CreateWaitlistRequestAsync(7, "Lungs");

        _transplantRepository.Verify(x => x.AddAsync(It.Is<Transplant>(t => t.OrganType == "Lung")), Times.Once);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorIsNotDeceased_ThrowsInvalidOperationException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new Patient { Id = 5, IsDonor = true });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetTopMatchesForDonorAsync(5, "Kidney"));
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorDoesNotExist_ThrowsInvalidOperationException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetTopMatchesForDonorAsync(5, "Kidney"));
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorIsNotRegistered_ThrowsInvalidOperationException()
    {
        Patient donor = MakeDeceasedDonor(5);
        donor.IsDonor = false;
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetTopMatchesForDonorAsync(5, "Kidney"));
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenReceiverDoesNotExist_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync((Patient?)null);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenReceiverBloodTypeIsMissing_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(bloodType: null));
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenReceiverRhIsMissing_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(rh: null));
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenReceiverHistoryIsMissing_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync((MedicalHistory?)null);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenBloodDoesNotMatch_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory(BloodType.A));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.B));
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.A, BloodType.B)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorBloodTypeIsMissing_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory(bloodType: null));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.B));
        _compatibilityService.Setup(x => x.IsBloodMatch(null, BloodType.B)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorHistoryIsMissingAtBloodCheck_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.B));
        _compatibilityService.Setup(x => x.IsBloodMatch(null, BloodType.B)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenRhDoesNotMatch_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory(BloodType.O, Rh.Positive));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.O, Rh.Negative));
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.O, BloodType.O)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(Rh.Positive, Rh.Negative)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorRhIsMissing_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory(BloodType.O, rh: null));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.O, Rh.Negative));
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.O, BloodType.O)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(null, Rh.Negative)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorHistoryIsMissingAtRhCheck_SkipsRequest()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.O, Rh.Negative));
        _compatibilityService.Setup(x => x.IsBloodMatch(null, BloodType.O)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(null, Rh.Negative)).Returns(false);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenMoreThanFiveMatchesExist_ReturnsFive()
    {
        Patient donor = MakeDeceasedDonor(5);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _compatibilityService.Setup(x => x.CalculateScore(It.IsAny<Patient>(), It.IsAny<Patient>())).Returns(50);
        _recordRepository.Setup(x => x.GetERVisitCountAsync(It.IsAny<int>(), It.IsAny<DateTime>())).ReturnsAsync(4);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync(
            Enumerable.Range(1, 6)
                .Select(id => new Transplant { TransplantId = id, ReceiverId = id + 10, OrganType = "Kidney", RequestDate = DateTime.UtcNow.AddDays(id) })
                .ToList());
        foreach (int receiverId in Enumerable.Range(11, 6))
        {
            _patientRepository.Setup(x => x.GetByIdAsync(receiverId)).ReturnsAsync(MakePatient(receiverId));
            _historyRepository.Setup(x => x.GetByPatientIdAsync(receiverId)).ReturnsAsync(MakeHistory());
        }

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenReceiverIsUrgent_AddsMaxScoreModifier()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory());
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _compatibilityService.Setup(x => x.CalculateScore(donor, receiver)).Returns(50);
        _recordRepository.Setup(x => x.GetERVisitCountAsync(7, It.IsAny<DateTime>())).ReturnsAsync(10);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<Transplant> result = await _sut.GetTopMatchesForDonorAsync(5, "Kidney");

        Assert.AreEqual(70, result[0].CompatibilityScore);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenOrganTypeHasWhitespace_TrimsBeforeLookup()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(MakeDeceasedDonor(5));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([]);

        await _sut.GetTopMatchesForDonorAsync(5, " Kidney ");

        _transplantRepository.Verify(x => x.GetWaitingByOrganAsync("Kidney"), Times.Once);
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_WhenReceiverExists_ReturnsReceiverName()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.A));
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<TransplantMatch> result = await _sut.GetTopMatchesAsDisplayModelsAsync(5, "Kidney");

        Assert.AreEqual("Jane Doe", result[0].ReceiverName);
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_WhenReceiverDisappears_ReturnsUnknownName()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.SetupSequence(x => x.GetByIdAsync(7))
            .ReturnsAsync(receiver)
            .ReturnsAsync((Patient?)null);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(7)).ReturnsAsync(MakeHistory(BloodType.A));
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<TransplantMatch> result = await _sut.GetTopMatchesAsDisplayModelsAsync(5, "Kidney");

        Assert.AreEqual("Unknown", result[0].ReceiverName);
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_WhenReceiverHistoryDisappears_ReturnsUnknownBloodType()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.SetupSequence(x => x.GetByPatientIdAsync(7))
            .ReturnsAsync(MakeHistory(BloodType.A))
            .ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<TransplantMatch> result = await _sut.GetTopMatchesAsDisplayModelsAsync(5, "Kidney");

        Assert.AreEqual("Unknown", result[0].BloodType);
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_WhenReceiverBloodTypeDisappears_ReturnsUnknownBloodType()
    {
        Patient donor = MakeDeceasedDonor(5);
        Patient receiver = MakePatient(7);
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(donor);
        _patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(receiver);
        _historyRepository.SetupSequence(x => x.GetByPatientIdAsync(7))
            .ReturnsAsync(MakeHistory(BloodType.A))
            .ReturnsAsync(MakeHistory(bloodType: null));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(MakeHistory());
        _compatibilityService.Setup(x => x.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);
        _transplantRepository.Setup(x => x.GetWaitingByOrganAsync("Kidney")).ReturnsAsync([
            new Transplant { TransplantId = 1, ReceiverId = 7, OrganType = "Kidney" }
        ]);

        List<TransplantMatch> result = await _sut.GetTopMatchesAsDisplayModelsAsync(5, "Kidney");

        Assert.AreEqual("Unknown", result[0].BloodType);
    }

    [TestMethod]
    public async Task IsUrgentAsync_WhenErVisitCountMeetsThreshold_ReturnsTrue()
    {
        _recordRepository.Setup(x => x.GetERVisitCountAsync(5, It.IsAny<DateTime>())).ReturnsAsync(10);

        bool result = await _sut.IsUrgentAsync(5);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientHasConditions_ReturnsWarningMessage()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(MakePatient(5));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(new MedicalHistory
        {
            Id = 11,
            PatientId = 5,
            ChronicConditions = new List<string> { "Asthma" }
        });

        string? result = await _sut.GetChronicWarningAsync(5);

        Assert.AreEqual("Patient has underlying conditions that may affect transplant success.", result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientHasNoConditions_ReturnsNull()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(MakePatient(5));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync(new MedicalHistory
        {
            Id = 11,
            PatientId = 5,
            ChronicConditions = new List<string>()
        });

        string? result = await _sut.GetChronicWarningAsync(5);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync((Patient?)null);

        string? result = await _sut.GetChronicWarningAsync(5);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenHistoryDoesNotExist_ReturnsNull()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(MakePatient(5));
        _historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync((MedicalHistory?)null);

        string? result = await _sut.GetChronicWarningAsync(5);

        Assert.IsNull(result);
    }
}
