using Common.API.Services;
using Common.Data.Models;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class TriageParametersServiceTests
{
    private Mock<ITriageParametersRepository> _repository = null!;
    private TriageParametersService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<ITriageParametersRepository>();
        _sut = new TriageParametersService(_repository.Object);
    }

    private static Triage_Parameters MakeParameters(int triageId = 1) => new()
    {
        Triage_ID = triageId,
        Consciousness = 1,
        Breathing = 1,
        Bleeding = 1,
        Injury_Type = 1,
        Pain_Level = 1
    };

    [TestMethod]
    public async Task GetAllAsync_WhenRepositoryReturnsTwoItems_ReturnsTwoItems()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([MakeParameters(1), MakeParameters(2)]);

        List<Triage_Parameters> result = await _sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsParameters_ReturnsSameInstance()
    {
        Triage_Parameters parameters = MakeParameters(7);
        _repository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(parameters);

        Triage_Parameters? result = await _sut.GetByIdAsync(7);

        Assert.AreSame(parameters, result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenCalled_DelegatesToRepository()
    {
        Triage_Parameters parameters = MakeParameters(3);
        _repository.Setup(x => x.CreateAsync(parameters)).ReturnsAsync(parameters);

        await _sut.CreateAsync(parameters);

        _repository.Verify(x => x.CreateAsync(parameters), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenCalled_DelegatesToRepository()
    {
        Triage_Parameters parameters = MakeParameters(4);
        _repository.Setup(x => x.UpdateAsync(4, parameters)).ReturnsAsync(true);

        await _sut.UpdateAsync(4, parameters);

        _repository.Verify(x => x.UpdateAsync(4, parameters), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.Setup(x => x.DeleteAsync(9)).ReturnsAsync(true);

        await _sut.DeleteAsync(9);

        _repository.Verify(x => x.DeleteAsync(9), Times.Once);
    }
}
