using Common.API.Services;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class TriageServiceTests
{
    private Mock<ITriageRepository> _repository = null!;
    private TriageService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<ITriageRepository>();
        _sut = new TriageService(_repository.Object);
    }

    private static Triage MakeTriage(int id = 1) => new()
    {
        Triage_ID = id,
        Visit_ID = id * 10,
        Triage_Level = 2,
        Specialization = "Neurology",
        Nurse_ID = 7
    };

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsTwoItems_ReturnsTwoItems()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([MakeTriage(1), MakeTriage(2)]);

        List<Triage> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsTriage_ReturnsSameInstance()
    {
        Triage triage = MakeTriage(4);
        _repository.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(triage);

        Triage? result = await _sut.GetByIdAsync(4);

        Assert.AreSame(triage, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        Triage triage = MakeTriage(5);
        _repository.Setup(x => x.CreateAsync(triage)).ReturnsAsync(triage);

        await _sut.CreateAsync(triage);

        _repository.Verify(x => x.CreateAsync(triage), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        Triage triage = MakeTriage(6);
        _repository.Setup(x => x.UpdateAsync(6, triage)).ReturnsAsync(true);

        await _sut.UpdateAsync(6, triage);

        _repository.Verify(x => x.UpdateAsync(6, triage), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.Setup(x => x.DeleteAsync(7)).ReturnsAsync(true);

        await _sut.DeleteAsync(7);

        _repository.Verify(x => x.DeleteAsync(7), Times.Once);
    }
}
