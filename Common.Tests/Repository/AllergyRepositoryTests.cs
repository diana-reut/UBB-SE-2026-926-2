using System.Threading.Tasks;
using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class AllergyRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);

    private static Allergy MakeAllergy(int id, string name, string? type = null, string? category = null) =>
        new() { Id = id, AllergyName = name, AllergyType = type, AllergyCategory = category };

    [TestMethod]
    public async Task GetAllergiesAsyncWhenNoAllergiesReturnsEmptyList()
    {
        await using var context = CreateContext();
        var sut = new AllergyRepository(context);

        var result = await sut.GetAllergiesAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncWhenAllergiesExistReturnsAllItems()
    {
        await using var context = CreateContext();
        context.Allergies.AddRange(
            MakeAllergy(1, "Peanuts"),
            MakeAllergy(2, "Penicillin"),
            MakeAllergy(3, "Latex"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetAllergiesAsync();

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncReturnsCorrectAllergyName()
    {
        await using var context = CreateContext();
        context.Allergies.Add(MakeAllergy(1, "Peanuts", type: "Food"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetAllergiesAsync();

        Assert.AreEqual("Peanuts", result[0].AllergyName);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncReturnsCorrectAllergyType()
    {
        await using var context = CreateContext();
        context.Allergies.Add(MakeAllergy(1, "Penicillin", type: "Drug"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetAllergiesAsync();

        Assert.AreEqual("Drug", result[0].AllergyType);
    }

    [TestMethod]
    public async Task GetByIdWhenAllergyExistsReturnsAllergy()
    {
        await using var context = CreateContext();
        context.Allergies.Add(MakeAllergy(1, "Peanuts"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = sut.GetById(1);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetByIdWhenAllergyExistsReturnsCorrectAllergy()
    {
        await using var context = CreateContext();
        context.Allergies.AddRange(
            MakeAllergy(1, "Peanuts"),
            MakeAllergy(2, "Penicillin"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = sut.GetById(2);

        Assert.AreEqual("Penicillin", result!.AllergyName);
    }

    [TestMethod]
    public async Task GetByIdWhenAllergyDoesNotExistReturnsNull()
    {
        await using var context = CreateContext();
        var sut = new AllergyRepository(context);

        var result = sut.GetById(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsyncWhenAllergyExistsReturnsAllergy()
    {
        await using var context = CreateContext();
        context.Allergies.Add(MakeAllergy(1, "Latex"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetByIdAsync(1);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsyncWhenAllergyExistsReturnsCorrectAllergy()
    {
        await using var context = CreateContext();
        context.Allergies.AddRange(
            MakeAllergy(1, "Latex"),
            MakeAllergy(2, "Shellfish", type: "Food", category: "Seafood"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetByIdAsync(2);

        Assert.AreEqual("Shellfish", result!.AllergyName);
    }

    [TestMethod]
    public async Task GetByIdAsyncWhenAllergyDoesNotExistReturnsNull()
    {
        await using var context = CreateContext();
        var sut = new AllergyRepository(context);

        var result = await sut.GetByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsyncReturnsCorrectCategory()
    {
        await using var context = CreateContext();
        context.Allergies.Add(MakeAllergy(1, "Shellfish", category: "Seafood"));
        await context.SaveChangesAsync();
        var sut = new AllergyRepository(context);

        var result = await sut.GetByIdAsync(1);

        Assert.AreEqual("Seafood", result!.AllergyCategory);
    }
}
