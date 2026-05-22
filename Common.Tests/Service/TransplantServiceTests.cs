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
            ChronicConditions = ["Asthma"]
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
            ChronicConditions = []
        });

        string? result = await _sut.GetChronicWarningAsync(5);

        Assert.IsNull(result);
    }
}
