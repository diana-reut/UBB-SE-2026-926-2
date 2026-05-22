using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class ExaminationServiceTests
{
    private Mock<IExaminationRepository> _repository = null!;
    private Mock<IERVisitRepository> _visitRepository = null!;
    private Mock<IERRoomRepository> _roomRepository = null!;
    private Mock<ITriageRepository> _triageRepository = null!;
    private Mock<ITriageParametersRepository> _triageParametersRepository = null!;
    private Mock<IPatientRepository> _patientRepository = null!;
    private ExaminationService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<IExaminationRepository>();
        _visitRepository = new Mock<IERVisitRepository>();
        _roomRepository = new Mock<IERRoomRepository>();
        _triageRepository = new Mock<ITriageRepository>();
        _triageParametersRepository = new Mock<ITriageParametersRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _sut = new ExaminationService(
            _repository.Object,
            _visitRepository.Object,
            _roomRepository.Object,
            _triageRepository.Object,
            _triageParametersRepository.Object,
            _patientRepository.Object);
    }

    private static Patient MakePatient(string cnp = "1234567890123") => new()
    {
        Id = 7,
        FirstName = "Jane",
        LastName = "Doe",
        Cnp = cnp,
        PhoneNo = "0700",
        EmergencyContact = "John",
        Dob = new DateTime(1990, 1, 1),
        Sex = Sex.F
    };

    [TestMethod]
    public async Task GetByVisitIdAsync_WhenMultipleExaminationsExist_ReturnsNewestFirst()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Examination { Exam_ID = 1, Visit_ID = 10, Exam_Time = new DateTime(2026, 1, 1) },
            new Examination { Exam_ID = 2, Visit_ID = 10, Exam_Time = new DateTime(2026, 1, 2) },
            new Examination { Exam_ID = 3, Visit_ID = 11, Exam_Time = new DateTime(2026, 1, 3) }
        ]);

        List<Examination> result = await _sut.GetByVisitIdAsync(10);

        Assert.AreEqual(2, result[0].Exam_ID);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Examination { Exam_ID = 1 },
            new Examination { Exam_ID = 2 }
        ]);

        List<Examination> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsExamination_ReturnsSameInstance()
    {
        Examination examination = new() { Exam_ID = 8 };
        _repository.Setup(x => x.GetByIdAsync(8)).ReturnsAsync(examination);

        Examination? result = await _sut.GetByIdAsync(8);

        Assert.AreSame(examination, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        Examination examination = new() { Exam_ID = 8 };
        _repository.Setup(x => x.CreateAsync(examination)).ReturnsAsync(examination);

        await _sut.CreateAsync(examination);

        _repository.Verify(x => x.CreateAsync(examination), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        Examination examination = new() { Exam_ID = 8 };
        _repository.Setup(x => x.UpdateAsync(8, examination)).ReturnsAsync(true);

        await _sut.UpdateAsync(8, examination);

        _repository.Verify(x => x.UpdateAsync(8, examination), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.Setup(x => x.DeleteAsync(8)).ReturnsAsync(true);

        await _sut.DeleteAsync(8);

        _repository.Verify(x => x.DeleteAsync(8), Times.Once);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenInRoomVisitHasNoLinkedRoom_ExcludesVisit()
    {
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.IN_ROOM, Arrival_date_time = new DateTime(2026, 1, 1) },
            new ER_Visit { Visit_ID = 2, Status = ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, Arrival_date_time = new DateTime(2026, 1, 2) }
        ]);

        List<ER_Visit> result = await _sut.GetEligibleVisitsAsync();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetEligibleVisitsAsync_WhenWaitingAndLinkedInRoomExist_OrdersByArrivalTime()
    {
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 1, Current_Visit_ID = 1 }
        ]);
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 2, Status = ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, Arrival_date_time = new DateTime(2026, 1, 2) },
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.IN_ROOM, Arrival_date_time = new DateTime(2026, 1, 1) }
        ]);

        List<ER_Visit> result = await _sut.GetEligibleVisitsAsync();

        Assert.AreEqual(1, result[0].Visit_ID);
    }

    [TestMethod]
    public async Task GetPatientHistoryAsync_WhenPatientHasNoVisits_ReturnsEmptyList()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        List<Examination> result = await _sut.GetPatientHistoryAsync("123");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPatientHistoryAsync_WhenVisitsExist_ReturnsNewestExaminationFirst()
    {
        _visitRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Patient_ID = "123" },
            new ER_Visit { Visit_ID = 2, Patient_ID = "123" }
        ]);
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Examination { Exam_ID = 1, Visit_ID = 1, Exam_Time = new DateTime(2026, 1, 1) },
            new Examination { Exam_ID = 2, Visit_ID = 2, Exam_Time = new DateTime(2026, 1, 3) }
        ]);

        List<Examination> result = await _sut.GetPatientHistoryAsync("123");

        Assert.AreEqual(2, result[0].Exam_ID);
    }

    [TestMethod]
    public async Task GetSummaryByVisitIdAsync_WhenNoExaminationExists_ReturnsNull()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        ERExaminationSummaryDto? result = await _sut.GetSummaryByVisitIdAsync(10);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetSummaryByVisitIdAsync_WhenPatientIsMissing_ReturnsNull()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Examination { Exam_ID = 1, Visit_ID = 10, Doctor_ID = 5, Exam_Time = DateTime.Today, Notes = "Stable" }]);
        _visitRepository.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ER_Visit { Visit_ID = 10, Patient_ID = "123", Arrival_date_time = DateTime.Today, Chief_Complaint = "Pain" });
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Triage { Triage_ID = 1, Visit_ID = 10 }]);

        ERExaminationSummaryDto? result = await _sut.GetSummaryByVisitIdAsync(10);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetSummaryByVisitIdAsync_WhenAllDataExists_ReturnsPatientFirstName()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Examination { Exam_ID = 1, Visit_ID = 10, Doctor_ID = 5, Exam_Time = DateTime.Today, Notes = "Stable" }]);
        _visitRepository.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ER_Visit { Visit_ID = 10, Patient_ID = "1234567890123", Arrival_date_time = DateTime.Today, Chief_Complaint = "Pain" });
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([MakePatient()]);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Triage { Triage_ID = 1, Visit_ID = 10, Triage_Level = 2, Specialization = "Neurology" }]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Triage_Parameters { Triage_ID = 1, Consciousness = 1, Breathing = 2, Bleeding = 1, Injury_Type = 1, Pain_Level = 2 }]);

        ERExaminationSummaryDto? result = await _sut.GetSummaryByVisitIdAsync(10);

        Assert.AreEqual("Jane", result!.FirstName);
    }
}
