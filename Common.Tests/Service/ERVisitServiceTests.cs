using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class ERVisitServiceTests
{
    private Mock<IERVisitRepository> _repository = null!;
    private Mock<IERRoomRepository> _roomRepository = null!;
    private Mock<ITriageRepository> _triageRepository = null!;
    private Mock<ITriageParametersRepository> _triageParametersRepository = null!;
    private Mock<ITransferLogRepository> _transferLogRepository = null!;
    private Mock<IPatientRepository> _patientRepository = null!;
    private ERVisitService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<IERVisitRepository>();
        _roomRepository = new Mock<IERRoomRepository>();
        _triageRepository = new Mock<ITriageRepository>();
        _triageParametersRepository = new Mock<ITriageParametersRepository>();
        _transferLogRepository = new Mock<ITransferLogRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _sut = new ERVisitService(
            _repository.Object,
            _roomRepository.Object,
            _triageRepository.Object,
            _triageParametersRepository.Object,
            _transferLogRepository.Object,
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
    public async Task AutoAssignHighestPriorityRoomAsync_WhenNoWaitingVisits_ReturnsFalse()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        bool result = await _sut.AutoAssignHighestPriorityRoomAsync();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1 },
            new ER_Visit { Visit_ID = 2 }
        ]);

        List<ER_Visit> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsVisit_ReturnsSameInstance()
    {
        ER_Visit visit = new() { Visit_ID = 4 };
        _repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(visit);

        ER_Visit? result = await _sut.GetByIdAsync(4);

        Assert.AreSame(visit, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        ER_Visit visit = new() { Visit_ID = 4 };
        _repository.Setup(x => x.CreateAsync(visit)).ReturnsAsync(visit);

        await _sut.CreateAsync(visit);

        _repository.Verify(x => x.CreateAsync(visit), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        ER_Visit visit = new() { Visit_ID = 4 };
        _repository.Setup(x => x.UpdateAsync(4, visit)).ReturnsAsync(true);

        await _sut.UpdateAsync(4, visit);

        _repository.Verify(x => x.UpdateAsync(4, visit), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.Setup(x => x.DeleteAsync(4)).ReturnsAsync(true);

        await _sut.DeleteAsync(4);

        _repository.Verify(x => x.DeleteAsync(4), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenNoAvailableRoom_ReturnsFalse()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Neurology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 2, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        bool result = await _sut.AutoAssignHighestPriorityRoomAsync();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenAvailableRoomExists_UpdatesRoomToOccupied()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Neurology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 2, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 5, Room_Type = ER_Room.RoomType.NeurologyRoom, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new ER_Room { Room_ID = 5, Room_Type = ER_Room.RoomType.NeurologyRoom, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(5, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(5, It.Is<ER_Room>(r => r.Availability_Status == ER_Room.RoomStatus.Occupied)), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenGeneralSurgeryIsRequired_UsesOperatingRoom()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "General Surgery", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 1, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 10, Room_Type = ER_Room.RoomType.OperatingRoom, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ER_Room { Room_ID = 10, Room_Type = ER_Room.RoomType.OperatingRoom, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(10, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(10, It.Is<ER_Room>(r => r.Room_Type == ER_Room.RoomType.OperatingRoom)), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenCriticalConsciousnessIsDetected_UsesTraumaBay()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Cardiology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 3, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 11, Room_Type = ER_Room.RoomType.TraumaBay, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(11)).ReturnsAsync(new ER_Room { Room_ID = 11, Room_Type = ER_Room.RoomType.TraumaBay, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(11, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(11, It.Is<ER_Room>(r => r.Room_Type == ER_Room.RoomType.TraumaBay)), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenPulmonologyIsRequired_UsesRespiratoryRoom()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Pulmonology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 1, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 12, Room_Type = ER_Room.RoomType.RespiratoryRoom, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(12)).ReturnsAsync(new ER_Room { Room_ID = 12, Room_Type = ER_Room.RoomType.RespiratoryRoom, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(12, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(12, It.Is<ER_Room>(r => r.Room_Type == ER_Room.RoomType.RespiratoryRoom)), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenOrthopedicInjuryIsDetected_UsesOrthopedicRoom()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Cardiology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 1, Breathing = 1, Bleeding = 1, Injury_Type = 2, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 13, Room_Type = ER_Room.RoomType.OrthopedicRoom, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(13)).ReturnsAsync(new ER_Room { Room_ID = 13, Room_Type = ER_Room.RoomType.OrthopedicRoom, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(13, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(13, It.Is<ER_Room>(r => r.Room_Type == ER_Room.RoomType.OrthopedicRoom)), Times.Once);
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_WhenNoSpecializedNeedExists_UsesGeneralRoom()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM, Arrival_date_time = DateTime.Today }
        ]);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage { Triage_ID = 1, Visit_ID = 1, Specialization = "Dermatology", Triage_Level = 1 }
        ]);
        _triageParametersRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new Triage_Parameters { Triage_ID = 1, Consciousness = 1, Breathing = 1, Bleeding = 1, Injury_Type = 1, Pain_Level = 1 }
        ]);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 14, Room_Type = ER_Room.RoomType.GeneralRoom, Availability_Status = ER_Room.RoomStatus.Available }
        ]);
        _roomRepository.Setup(x => x.GetByIdAsync(14)).ReturnsAsync(new ER_Room { Room_ID = 14, Room_Type = ER_Room.RoomType.GeneralRoom, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(14, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.AutoAssignHighestPriorityRoomAsync();

        _roomRepository.Verify(x => x.UpdateAsync(14, It.Is<ER_Room>(r => r.Room_Type == ER_Room.RoomType.GeneralRoom)), Times.Once);
    }

    [TestMethod]
    public async Task AssignRoomAsync_WhenRoomIsMissing_ThrowsInvalidOperationException()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync((ER_Room?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AssignRoomAsync(1, 4));
    }

    [TestMethod]
    public async Task AssignRoomAsync_WhenRoomIsUnavailable_ThrowsInvalidOperationException()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new ER_Room { Room_ID = 4, Availability_Status = ER_Room.RoomStatus.Occupied });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AssignRoomAsync(1, 4));
    }

    [TestMethod]
    public async Task AssignRoomAsync_WhenVisitIsMissing_ThrowsInvalidOperationException()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new ER_Room { Room_ID = 4, Availability_Status = ER_Room.RoomStatus.Available });
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((ER_Visit?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AssignRoomAsync(1, 4));
    }

    [TestMethod]
    public async Task AssignRoomAsync_WhenVisitStatusIsInvalid_ThrowsInvalidOperationException()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new ER_Room { Room_ID = 4, Availability_Status = ER_Room.RoomStatus.Available });
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.REGISTERED });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AssignRoomAsync(1, 4));
    }

    [TestMethod]
    public async Task AssignRoomAsync_WhenVisitIsWaitingForRoom_UpdatesVisitToInRoom()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new ER_Room { Room_ID = 4, Availability_Status = ER_Room.RoomStatus.Available });
        _roomRepository.Setup(x => x.UpdateAsync(4, It.IsAny<ER_Room>())).ReturnsAsync(true);
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);

        await _sut.AssignRoomAsync(1, 4);

        _repository.Verify(x => x.UpdateAsync(1, It.Is<ER_Visit>(v => v.Status == ER_Visit.VisitStatus.IN_ROOM)), Times.Once);
    }

    [TestMethod]
    public async Task TransferVisitAsync_WhenVisitCanBeTransferred_LogsSuccessfulTransfer()
    {
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.IN_EXAMINATION });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);

        await _sut.TransferVisitAsync(1);

        _transferLogRepository.Verify(x => x.CreateAsync(It.Is<Transfer_Log>(t => t.Status == "SUCCESS")), Times.Once);
    }

    [TestMethod]
    public async Task TransferVisitAsync_WhenUpdateFails_LogsFailedTransfer()
    {
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.REGISTERED });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.TransferVisitAsync(1));

        _transferLogRepository.Verify(x => x.CreateAsync(It.Is<Transfer_Log>(t => t.Status == "FAILED")), Times.Once);
    }

    [TestMethod]
    public async Task RetryTransferAsync_WhenVisitHasPatient_MarksPatientAsTransferred()
    {
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.IN_EXAMINATION, Patient_ID = "1234567890123" });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([MakePatient()]);
        _patientRepository.Setup(x => x.UpdateAsync(It.IsAny<Patient>())).Returns(Task.CompletedTask);

        await _sut.RetryTransferAsync(1);

        _patientRepository.Verify(x => x.UpdateAsync(It.Is<Patient>(p => p.Transferred)), Times.Once);
    }

    [TestMethod]
    public async Task CloseVisitAsync_WhenOccupiedRoomExists_UpdatesRoomToCleaning()
    {
        _repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Visit { Visit_ID = 1, Status = ER_Visit.VisitStatus.IN_EXAMINATION });
        _repository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Visit>())).ReturnsAsync(true);
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 5, Availability_Status = ER_Room.RoomStatus.Occupied, Current_Visit_ID = 1 }
        ]);
        _roomRepository.Setup(x => x.UpdateAsync(5, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.CloseVisitAsync(1);

        _roomRepository.Verify(x => x.UpdateAsync(5, It.Is<ER_Room>(r => r.Availability_Status == ER_Room.RoomStatus.Cleaning)), Times.Once);
    }
}
