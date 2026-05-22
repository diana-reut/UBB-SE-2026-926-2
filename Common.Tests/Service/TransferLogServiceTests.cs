using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class TransferLogServiceTests
{
    private Mock<ITransferLogRepository> _repository = null!;
    private Mock<IERVisitRepository> _visitRepository = null!;
    private Mock<IPatientRepository> _patientRepository = null!;
    private TransferLogService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<ITransferLogRepository>();
        _visitRepository = new Mock<IERVisitRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _sut = new TransferLogService(_repository.Object, _visitRepository.Object, _patientRepository.Object);
    }

    [TestMethod]
    public async Task GetByVisitIdAsync_WhenMatchingLogsExist_ReturnsNewestLogFirst()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Transfer_Log { Transfer_ID = 1, Visit_ID = 10, Transfer_Time = new DateTime(2026, 1, 1), Target_System = "A", Status = "SUCCESS" },
            new Transfer_Log { Transfer_ID = 2, Visit_ID = 10, Transfer_Time = new DateTime(2026, 1, 2), Target_System = "B", Status = "SUCCESS" },
            new Transfer_Log { Transfer_ID = 3, Visit_ID = 20, Transfer_Time = new DateTime(2026, 1, 3), Target_System = "C", Status = "SUCCESS" }
        ]);

        List<Transfer_Log> result = await _sut.GetByVisitIdAsync(10);

        Assert.AreEqual(2, result[0].Transfer_ID);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Transfer_Log { Transfer_ID = 1, Visit_ID = 10, Transfer_Time = DateTime.Today, Target_System = "A", Status = "SUCCESS" },
            new Transfer_Log { Transfer_ID = 2, Visit_ID = 11, Transfer_Time = DateTime.Today, Target_System = "B", Status = "SUCCESS" }
        ]);

        List<Transfer_Log> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsLog_ReturnsSameInstance()
    {
        Transfer_Log log = new() { Transfer_ID = 4, Visit_ID = 10, Transfer_Time = DateTime.Today, Target_System = "A", Status = "SUCCESS" };
        _repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(log);

        Transfer_Log? result = await _sut.GetByIdAsync(4);

        Assert.AreSame(log, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        Transfer_Log log = new() { Transfer_ID = 4, Visit_ID = 10, Transfer_Time = DateTime.Today, Target_System = "A", Status = "SUCCESS" };
        _repository.Setup(x => x.CreateAsync(log)).ReturnsAsync(log);

        await _sut.CreateAsync(log);

        _repository.Verify(x => x.CreateAsync(log), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        Transfer_Log log = new() { Transfer_ID = 4, Visit_ID = 10, Transfer_Time = DateTime.Today, Target_System = "A", Status = "SUCCESS" };
        _repository.Setup(x => x.UpdateAsync(4, log)).ReturnsAsync(true);

        await _sut.UpdateAsync(4, log);

        _repository.Verify(x => x.UpdateAsync(4, log), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.Setup(x => x.DeleteAsync(4)).ReturnsAsync(true);

        await _sut.DeleteAsync(4);

        _repository.Verify(x => x.DeleteAsync(4), Times.Once);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenVisitIsNotInExamination_ExcludesVisit()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Patient_ID = "123", Status = ER_Visit.VisitStatus.REGISTERED },
            new ER_Visit { Visit_ID = 2, Patient_ID = "456", Status = ER_Visit.VisitStatus.IN_EXAMINATION }
        ]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);

        List<ERTransferEligibleVisitDto> result = await _sut.GetEligibleVisitsAsync();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenPatientExists_UsesPatientFullName()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 2, Patient_ID = "456", Status = ER_Visit.VisitStatus.IN_EXAMINATION }
        ]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([
            new Patient { Id = 7, FirstName = "Jane", LastName = "Doe", Cnp = "456", PhoneNo = "0700", EmergencyContact = "John", Dob = new DateTime(1990, 1, 1), Sex = Common.Data.Entity.Enums.Sex.F }
        ]);

        List<ERTransferEligibleVisitDto> result = await _sut.GetEligibleVisitsAsync();

        Assert.AreEqual("Jane Doe", result[0].PatientName);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenPatientDoesNotExist_UsesVisitPatientId()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 2, Patient_ID = "456", Status = ER_Visit.VisitStatus.IN_EXAMINATION }
        ]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);

        List<ERTransferEligibleVisitDto> result = await _sut.GetEligibleVisitsAsync();

        Assert.AreEqual("456", result[0].PatientName);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenPatientIsMissing_UsesTransferredFalse()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 2, Patient_ID = "456", Status = ER_Visit.VisitStatus.IN_EXAMINATION }
        ]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);

        List<ERTransferEligibleVisitDto> result = await _sut.GetEligibleVisitsAsync();

        Assert.IsFalse(result[0].Transferred);
    }
}
