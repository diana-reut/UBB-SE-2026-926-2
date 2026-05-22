using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class ERRoomServiceTests
{
    private Mock<IERRoomRepository> _roomRepository = null!;
    private Mock<IERVisitRepository> _visitRepository = null!;
    private Mock<ITriageRepository> _triageRepository = null!;
    private Mock<IPatientRepository> _patientRepository = null!;
    private ERRoomService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _roomRepository = new Mock<IERRoomRepository>();
        _visitRepository = new Mock<IERVisitRepository>();
        _triageRepository = new Mock<ITriageRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _sut = new ERRoomService(
            _roomRepository.Object,
            _visitRepository.Object,
            _triageRepository.Object,
            _patientRepository.Object);
    }

    [TestMethod]
    public async Task GetByStatusAsync_WhenStatusCaseDiffers_ReturnsMatchingRoom()
    {
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 1, Availability_Status = ER_Room.RoomStatus.Available },
            new ER_Room { Room_ID = 2, Availability_Status = ER_Room.RoomStatus.Cleaning }
        ]);

        List<ER_Room> result = await _sut.GetByStatusAsync("available");

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
    {
        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new ER_Room { Room_ID = 1 },
            new ER_Room { Room_ID = 2 }
        ]);

        List<ER_Room> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsRoom_ReturnsSameInstance()
    {
        ER_Room room = new() { Room_ID = 3 };
        _roomRepository.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(room);

        ER_Room? result = await _sut.GetByIdAsync(3);

        Assert.AreSame(room, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        ER_Room room = new() { Room_ID = 3 };
        _roomRepository.Setup(x => x.CreateAsync(room)).ReturnsAsync(room);

        await _sut.CreateAsync(room);

        _roomRepository.Verify(x => x.CreateAsync(room), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        ER_Room room = new() { Room_ID = 3 };
        _roomRepository.Setup(x => x.UpdateAsync(3, room)).ReturnsAsync(true);

        await _sut.UpdateAsync(3, room);

        _roomRepository.Verify(x => x.UpdateAsync(3, room), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _roomRepository.Setup(x => x.DeleteAsync(3)).ReturnsAsync(true);

        await _sut.DeleteAsync(3);

        _roomRepository.Verify(x => x.DeleteAsync(3), Times.Once);
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_WhenRoomDoesNotExist_ThrowsInvalidOperationException()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync((ER_Room?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetVisitDetailsAsync(4));
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_WhenRoomHasNoCurrentVisit_ReturnsNull()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Room { Room_ID = 1, Current_Visit_ID = null });

        ERRoomVisitDetailsDto? result = await _sut.GetVisitDetailsAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_WhenTriageExists_ReturnsTriageForVisit()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Room { Room_ID = 1, Current_Visit_ID = 10 });
        _visitRepository.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ER_Visit { Visit_ID = 10, Patient_ID = "123" });
        _patientRepository.Setup(x => x.SearchAsync(It.IsAny<PatientFilter>())).ReturnsAsync([]);
        _triageRepository.Setup(x => x.GetAllAsync()).ReturnsAsync([new Triage { Triage_ID = 77, Visit_ID = 10 }]);

        ERRoomVisitDetailsDto? result = await _sut.GetVisitDetailsAsync(1);

        Assert.AreEqual(77, result!.Triage!.Triage_ID);
    }

    [TestMethod]
    public async Task MarkRoomAsCleaningAsync_WhenActiveVisitExists_UpdatesVisitToWaitingForRoom()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ER_Room
        {
            Room_ID = 1,
            Availability_Status = ER_Room.RoomStatus.Occupied,
            Current_Visit_ID = 10
        });
        _visitRepository.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ER_Visit
        {
            Visit_ID = 10,
            Status = ER_Visit.VisitStatus.IN_ROOM
        });
        _roomRepository.Setup(x => x.UpdateAsync(1, It.IsAny<ER_Room>())).ReturnsAsync(true);
        _visitRepository.Setup(x => x.UpdateAsync(10, It.IsAny<ER_Visit>())).ReturnsAsync(true);

        await _sut.MarkRoomAsCleaningAsync(1);

        _visitRepository.Verify(x => x.UpdateAsync(10, It.Is<ER_Visit>(v => v.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)), Times.Once);
    }

    [TestMethod]
    public async Task MarkRoomAsAvailableAsync_WhenCalled_UpdatesRoomToAvailable()
    {
        _roomRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new ER_Room
        {
            Room_ID = 5,
            Availability_Status = ER_Room.RoomStatus.Cleaning
        });
        _roomRepository.Setup(x => x.UpdateAsync(5, It.IsAny<ER_Room>())).ReturnsAsync(true);

        await _sut.MarkRoomAsAvailableAsync(5);

        _roomRepository.Verify(x => x.UpdateAsync(5, It.Is<ER_Room>(r => r.Availability_Status == ER_Room.RoomStatus.Available)), Times.Once);
    }
}
