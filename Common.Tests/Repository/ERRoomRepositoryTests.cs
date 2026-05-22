using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class ERRoomRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ER_Room MakeRoom(int id = 1, string status = "Available") => new()
    {
        Room_ID = id,
        Room_Type = ER_Room.RoomType.GeneralRoom,
        Availability_Status = status
    };

    [TestMethod]
    public async Task GetAllAsync_WhenRoomsExist_ReturnsAllItems()
    {
        await using var context = CreateContext();
        context.ERRooms.AddRange(MakeRoom(1), MakeRoom(2));
        await context.SaveChangesAsync();
        var sut = new ERRoomRepository(context);

        List<ER_Room> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRoomExists_ReturnsMatchingRoom()
    {
        await using var context = CreateContext();
        context.ERRooms.Add(MakeRoom(8, ER_Room.RoomStatus.Cleaning));
        await context.SaveChangesAsync();
        var sut = new ERRoomRepository(context);

        ER_Room? result = await sut.GetByIdAsync(8);

        Assert.AreEqual(ER_Room.RoomStatus.Cleaning, result!.Availability_Status);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenRoomExists_UpdatesCurrentVisitId()
    {
        await using var context = CreateContext();
        context.ERRooms.Add(MakeRoom(8));
        await context.SaveChangesAsync();
        var sut = new ERRoomRepository(context);

        await sut.UpdateAsync(8, new ER_Room { Room_ID = 99, Room_Type = ER_Room.RoomType.GeneralRoom, Availability_Status = ER_Room.RoomStatus.Occupied, Current_Visit_ID = 44 });

        Assert.AreEqual(44, context.ERRooms.Single().Current_Visit_ID);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new ERRoomRepository(context);

        bool result = await sut.DeleteAsync(5);

        Assert.IsFalse(result);
    }
}
