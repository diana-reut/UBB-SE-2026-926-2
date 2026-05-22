using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class TriageRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Triage MakeTriage(int id = 1, int level = 5) => new()
    {
        Triage_ID = id,
        Visit_ID = id * 10,
        Triage_Level = level,
        Specialization = "General",
        Nurse_ID = 7
    };

    [TestMethod]
    public async Task GetAllAsync_WhenTriagesExist_ReturnsAllItems()
    {
        await using var context = CreateContext();
        context.Triages.AddRange(MakeTriage(1), MakeTriage(2));
        await context.SaveChangesAsync();
        var sut = new TriageRepository(context);

        List<Triage> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenTriageExists_ReturnsMatchingTriage()
    {
        await using var context = CreateContext();
        context.Triages.Add(MakeTriage(8, level: 2));
        await context.SaveChangesAsync();
        var sut = new TriageRepository(context);

        Triage? result = await sut.GetByIdAsync(8);

        Assert.AreEqual(2, result!.Triage_Level);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenTriageExists_UpdatesSpecialization()
    {
        await using var context = CreateContext();
        context.Triages.Add(MakeTriage(8));
        await context.SaveChangesAsync();
        var sut = new TriageRepository(context);

        await sut.UpdateAsync(8, new Triage { Triage_ID = 99, Visit_ID = 80, Triage_Level = 3, Specialization = "Neurology", Nurse_ID = 4 });

        Assert.AreEqual("Neurology", context.Triages.Single().Specialization);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenTriageDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new TriageRepository(context);

        bool result = await sut.DeleteAsync(5);

        Assert.IsFalse(result);
    }
}
