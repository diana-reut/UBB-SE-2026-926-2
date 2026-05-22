using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class UserRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [TestMethod]
    public async Task GetByUsernameAsync_WhenUserExists_ReturnsMatchingUser()
    {
        await using var context = CreateContext();
        context.Users.Add(new User { Username = "alice", PasswordHash = "hash", Role = "Admin" });
        await context.SaveChangesAsync();
        var sut = new UserRepository(context);

        User? result = await sut.GetByUsernameAsync("alice");

        Assert.AreEqual("alice", result!.Username);
    }

    [TestMethod]
    public async Task ExistsByUsernameAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new UserRepository(context);

        bool result = await sut.ExistsByUsernameAsync("ghost");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenUserIsValid_PersistsUser()
    {
        await using var context = CreateContext();
        var sut = new UserRepository(context);

        await sut.CreateAsync(new User { Username = "alice", PasswordHash = "hash", Role = "Doctor" });

        Assert.AreEqual(1, context.Users.Count());
    }
}
