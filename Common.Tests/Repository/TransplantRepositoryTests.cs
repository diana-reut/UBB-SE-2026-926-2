using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class TransplantRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Transplant MakeTransplant(
        int id = 1,
        string organType = "Kidney",
        TransplantStatus status = TransplantStatus.Pending,
        float score = 0) => new()
        {
            TransplantId = id,
            ReceiverId = id * 10,
            OrganType = organType,
            RequestDate = new DateTime(2026, 1, id),
            Status = status,
            CompatibilityScore = score
        };

    [TestMethod]
    public async Task GetWaitingByOrganAsync_WhenOrganAliasIsLung_ReturnsLungsAndLung()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(
            MakeTransplant(1, "Lung"),
            MakeTransplant(2, "Lungs"),
            MakeTransplant(3, "Kidney"));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetWaitingByOrganAsync("Lung");

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task AddAsync_WhenTransplantIsValid_PersistsTransplant()
    {
        await using var context = CreateContext();
        var sut = new TransplantRepository(context);

        await sut.AddAsync(MakeTransplant(1));

        Assert.AreEqual(1, context.Transplants.Count());
    }

    [TestMethod]
    public void Add_WhenTransplantIsValid_PersistsTransplant()
    {
        using var context = CreateContext();
        var sut = new TransplantRepository(context);

        sut.Add(MakeTransplant(1));

        Assert.AreEqual(1, context.Transplants.Count());
    }

    [TestMethod]
    public async Task GetAllAsync_WhenTransplantsExist_ReturnsAllTransplants()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(MakeTransplant(1), MakeTransplant(2));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetWaitingByOrganAsync_WhenOrganTypeContainsWhitespace_TrimsOrganType()
    {
        await using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1, "Kidney"));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetWaitingByOrganAsync(" Kidney ");

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetWaitingByOrgan_WhenMatchingTransplantsExist_ReturnsMatches()
    {
        using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1, "Kidney"));
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        List<Transplant> result = sut.GetWaitingByOrgan("Kidney");

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopMatchesAsync_WhenPendingMatchesExist_ReturnsHighestScoreFirst()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(
            MakeTransplant(1, "Kidney", score: 70),
            MakeTransplant(2, "Kidney", score: 95),
            MakeTransplant(3, "Kidney", status: TransplantStatus.Scheduled, score: 99));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetTopMatchesAsync("Kidney");

        Assert.AreEqual(2, result[0].TransplantId);
    }

    [TestMethod]
    public async Task GetTopMatchesAsync_WhenMoreThanFiveMatchesExist_ReturnsOnlyFiveMatches()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(
            MakeTransplant(1, score: 10),
            MakeTransplant(2, score: 20),
            MakeTransplant(3, score: 30),
            MakeTransplant(4, score: 40),
            MakeTransplant(5, score: 50),
            MakeTransplant(6, score: 60));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetTopMatchesAsync("Kidney");

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenAssigningDonor_ChangesStatusToScheduled()
    {
        await using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        await sut.UpdateAsync(1, 77, 88);

        Assert.AreEqual(TransplantStatus.Scheduled, context.Transplants.Single().Status);
    }

    [TestMethod]
    public void Update_WhenAssigningDonor_StoresCompatibilityScore()
    {
        using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1));
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        sut.Update(1, 77, 88);

        Assert.AreEqual(88f, context.Transplants.Single().CompatibilityScore);
    }

    [TestMethod]
    public async Task GetByReceiverIdAsync_WhenMatchesExist_ReturnsReceiverMatches()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(
            MakeTransplant(1),
            MakeTransplant(2),
            new Transplant { TransplantId = 3, ReceiverId = 99, OrganType = "Kidney", RequestDate = new DateTime(2026, 1, 3), Status = TransplantStatus.Pending });
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetByReceiverIdAsync(10);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetTopMatches_WhenMatchingTransplantsExist_ReturnsMatches()
    {
        using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1, score: 70));
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        List<Transplant> result = sut.GetTopMatches("Kidney");

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetByReceiverId_WhenMatchingTransplantsExist_ReturnsMatches()
    {
        using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1));
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        List<Transplant> result = sut.GetByReceiverId(10);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetByDonorIdAsync_WhenMatchesExist_ReturnsDonorMatches()
    {
        await using var context = CreateContext();
        context.Transplants.AddRange(
            new Transplant { TransplantId = 1, ReceiverId = 10, DonorId = 77, OrganType = "Kidney", RequestDate = new DateTime(2026, 1, 1), Status = TransplantStatus.Pending },
            new Transplant { TransplantId = 2, ReceiverId = 20, DonorId = 88, OrganType = "Kidney", RequestDate = new DateTime(2026, 1, 2), Status = TransplantStatus.Pending });
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        List<Transplant> result = await sut.GetByDonorIdAsync(77);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetByDonorId_WhenMatchingTransplantsExist_ReturnsMatches()
    {
        using var context = CreateContext();
        context.Transplants.Add(new Transplant { TransplantId = 1, ReceiverId = 10, DonorId = 77, OrganType = "Kidney", RequestDate = new DateTime(2026, 1, 1), Status = TransplantStatus.Pending });
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        List<Transplant> result = sut.GetByDonorId(77);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenTransplantDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var sut = new TransplantRepository(context);

        Transplant? result = await sut.GetByIdAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenTransplantExists_UpdatesOrganType()
    {
        await using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1, "Kidney"));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        await sut.UpdateAsync(1, MakeTransplant(9, "Liver", TransplantStatus.Completed, 55));

        Assert.AreEqual("Liver", context.Transplants.Single().OrganType);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenTransplantDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new TransplantRepository(context);

        bool result = await sut.UpdateAsync(9, MakeTransplant(9));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenTransplantExists_RemovesTransplant()
    {
        await using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1));
        await context.SaveChangesAsync();
        var sut = new TransplantRepository(context);

        await sut.DeleteAsync(1);

        Assert.AreEqual(0, context.Transplants.Count());
    }

    [TestMethod]
    public async Task DeleteAsync_WhenTransplantDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new TransplantRepository(context);

        bool result = await sut.DeleteAsync(1);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetById_WhenTransplantExists_ReturnsTransplant()
    {
        using var context = CreateContext();
        context.Transplants.Add(MakeTransplant(1));
        context.SaveChanges();
        var sut = new TransplantRepository(context);

        Transplant? result = sut.GetById(1);

        Assert.IsNotNull(result);
    }
}
