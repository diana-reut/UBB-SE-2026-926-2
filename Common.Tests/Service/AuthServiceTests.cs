using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Repository;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class AuthServiceTests
{
    private Mock<IUserRepository> _repository = null!;
    private AuthService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<IUserRepository>();
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "this-is-a-very-long-secret-key-for-tests",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
        _sut = new AuthService(_repository.Object, config);
    }

    private static User MakeUser(string username = "alice", string password = "secret", string role = "Doctor")
    {
        var repo = new Mock<IUserRepository>();
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Secret"] = "this-is-a-very-long-secret-key-for-tests" })
            .Build();
        var service = new AuthService(repo.Object, config);
        return service.RegisterAsync(new RegisterDto
        {
            Username = username,
            Password = password,
            Role = role
        }).GetAwaiter().GetResult() switch
        {
            var response => new User { Id = 9, Username = response.Username, Role = response.Role, PasswordHash = repo.Invocations.Count > 0 ? ((User)repo.Invocations[1].Arguments[0]).PasswordHash : string.Empty }
        };
    }

    [TestMethod]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        _repository.Setup(x => x.GetByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(new LoginDto
        {
            Username = "ghost",
            Password = "pw"
        }));
    }

    [TestMethod]
    public async Task LoginAsync_WhenPasswordIsInvalid_ThrowsUnauthorizedAccessException()
    {
        _repository.Setup(x => x.GetByUsernameAsync("alice")).ReturnsAsync(new User
        {
            Id = 1,
            Username = "alice",
            PasswordHash = Convert.ToBase64String(new byte[48]),
            Role = "Doctor"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(new LoginDto
        {
            Username = "alice",
            Password = "pw"
        }));
    }

    [TestMethod]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsRequestedUsername()
    {
        User createdUser = MakeUser();
        _repository.Setup(x => x.GetByUsernameAsync("alice")).ReturnsAsync(createdUser);

        AuthResponseDto result = await _sut.LoginAsync(new LoginDto
        {
            Username = "alice",
            Password = "secret"
        });

        Assert.AreEqual("alice", result.Username);
    }

    [TestMethod]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ThrowsArgumentException()
    {
        _repository.Setup(x => x.ExistsByUsernameAsync("alice")).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.RegisterAsync(new RegisterDto
        {
            Username = "alice",
            Password = "secret"
        }));
    }

    [TestMethod]
    public async Task RegisterAsync_WhenUsernameIsAvailable_ReturnsRequestedRole()
    {
        _repository.Setup(x => x.ExistsByUsernameAsync("alice")).ReturnsAsync(false);
        _repository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User user) => user);

        AuthResponseDto result = await _sut.RegisterAsync(new RegisterDto
        {
            Username = "alice",
            Password = "secret",
            Role = "Admin"
        });

        Assert.AreEqual("Admin", result.Role);
    }

    [TestMethod]
    public async Task RegisterAsync_WhenUsernameIsAvailable_PersistsHashedPassword()
    {
        _repository.Setup(x => x.ExistsByUsernameAsync("alice")).ReturnsAsync(false);
        _repository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User user) => user);

        await _sut.RegisterAsync(new RegisterDto
        {
            Username = "alice",
            Password = "secret",
            Role = "Admin"
        });

        _repository.Verify(x => x.CreateAsync(It.Is<User>(u => u.PasswordHash != "secret")), Times.Once);
    }
}
