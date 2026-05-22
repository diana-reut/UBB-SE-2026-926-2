using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class TriageParametersRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Triage_Parameters MakeParameters(int triageId = 1, int consciousness = 1) => new()
    {
        Triage_ID = triageId,
        Consciousness = consciousness,
        Breathing = 1,
        Bleeding = 1,
        Injury_Type = 1,
        Pain_Level = 1
    };

    [TestMethod]
    public async Task GetAllAsync_WhenParametersExist_ReturnsAllItems()
    {
        await using var context = CreateContext();
        context.TriageParameters.AddRange(MakeParameters(1), MakeParameters(2));
        await context.SaveChangesAsync();
        var sut = new TriageParametersRepository(context);

        List<Triage_Parameters> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenParametersExist_ReturnsMatchingParameters()
    {
        await using var context = CreateContext();
        context.TriageParameters.Add(MakeParameters(3, consciousness: 2));
        await context.SaveChangesAsync();
        var sut = new TriageParametersRepository(context);

        Triage_Parameters? result = await sut.GetByIdAsync(3);

        Assert.AreEqual(2, result!.Consciousness);
    }

    [TestMethod]
    public async Task CreateAsync_WhenTriageIdAlreadyExists_UpdatesExistingParameters()
    {
        await using var context = CreateContext();
        context.TriageParameters.Add(MakeParameters(3, consciousness: 1));
        await context.SaveChangesAsync();
        var sut = new TriageParametersRepository(context);

        await sut.CreateAsync(MakeParameters(3, consciousness: 3));

        Assert.AreEqual(3, context.TriageParameters.Single().Consciousness);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenParametersDoNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new TriageParametersRepository(context);

        bool result = await sut.UpdateAsync(9, MakeParameters(9));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenParametersExist_RemovesParameters()
    {
        await using var context = CreateContext();
        context.TriageParameters.Add(MakeParameters(5));
        await context.SaveChangesAsync();
        var sut = new TriageParametersRepository(context);

        await sut.DeleteAsync(5);

        Assert.AreEqual(0, context.TriageParameters.Count());
    }
}
