using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class ERVisitRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ER_Visit MakeVisit(int id = 1, string status = "REGISTERED") => new()
    {
        Visit_ID = id,
        Patient_ID = $"P{id}",
        Arrival_date_time = new DateTime(2026, 1, id),
        Chief_Complaint = "Pain",
        Status = status
    };

    [TestMethod]
    public async Task GetAllAsync_WhenVisitsExist_ReturnsAllItems()
    {
        await using var context = CreateContext();
        context.ERVisits.AddRange(MakeVisit(1), MakeVisit(2));
        await context.SaveChangesAsync();
        var sut = new ERVisitRepository(context);

        List<ER_Visit> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenVisitExists_ReturnsMatchingVisit()
    {
        await using var context = CreateContext();
        context.ERVisits.Add(MakeVisit(8, ER_Visit.VisitStatus.IN_ROOM));
        await context.SaveChangesAsync();
        var sut = new ERVisitRepository(context);

        ER_Visit? result = await sut.GetByIdAsync(8);

        Assert.AreEqual(ER_Visit.VisitStatus.IN_ROOM, result!.Status);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenVisitExists_UpdatesChiefComplaint()
    {
        await using var context = CreateContext();
        context.ERVisits.Add(MakeVisit(8));
        await context.SaveChangesAsync();
        var sut = new ERVisitRepository(context);

        await sut.UpdateAsync(8, new ER_Visit { Visit_ID = 99, Patient_ID = "P8", Arrival_date_time = DateTime.Today, Chief_Complaint = "Headache", Status = ER_Visit.VisitStatus.TRIAGED });

        Assert.AreEqual("Headache", context.ERVisits.Single().Chief_Complaint);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenVisitDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new ERVisitRepository(context);

        bool result = await sut.DeleteAsync(5);

        Assert.IsFalse(result);
    }
}
