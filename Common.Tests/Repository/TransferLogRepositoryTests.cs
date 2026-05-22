using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class TransferLogRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Transfer_Log MakeLog(int id = 1, string status = "SUCCESS") => new()
    {
        Transfer_ID = id,
        Visit_ID = 10,
        Transfer_Time = new DateTime(2026, 1, id),
        Target_System = "Patient Management",
        Status = status
    };

    [TestMethod]
    public async Task GetAllAsync_WhenNoLogsExist_ReturnsEmptyList()
    {
        await using var context = CreateContext();
        var sut = new TransferLogRepository(context);

        List<Transfer_Log> result = await sut.GetAllAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenLogExists_ReturnsMatchingLog()
    {
        await using var context = CreateContext();
        context.TransferLogs.Add(MakeLog(3));
        await context.SaveChangesAsync();
        var sut = new TransferLogRepository(context);

        Transfer_Log? result = await sut.GetByIdAsync(3);

        Assert.AreEqual(3, result!.Transfer_ID);
    }

    [TestMethod]
    public async Task CreateAsync_WhenLogIsValid_PersistsLog()
    {
        await using var context = CreateContext();
        var sut = new TransferLogRepository(context);

        await sut.CreateAsync(MakeLog());

        Assert.AreEqual(1, context.TransferLogs.Count());
    }

    [TestMethod]
    public async Task UpdateAsync_WhenLogExists_UpdatesStatus()
    {
        await using var context = CreateContext();
        context.TransferLogs.Add(MakeLog(1, "RETRYING"));
        await context.SaveChangesAsync();
        var sut = new TransferLogRepository(context);

        await sut.UpdateAsync(1, MakeLog(2, "FAILED"));

        Assert.AreEqual("FAILED", context.TransferLogs.Single().Status);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenLogDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new TransferLogRepository(context);

        bool result = await sut.DeleteAsync(99);

        Assert.IsFalse(result);
    }
}
