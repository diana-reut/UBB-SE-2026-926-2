using System.Collections.Generic;
using System.Threading.Tasks;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class AllergyServiceTests
{
    private Mock<IAllergyRepository> _repository = null!;
    private AllergyService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<IAllergyRepository>();
        _sut = new AllergyService(_repository.Object);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncWhenRepositoryReturnsEmptyListReturnsEmptyList()
    {
        _repository.Setup(r => r.GetAllergiesAsync()).ReturnsAsync([]);

        var result = await _sut.GetAllergiesAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncWhenRepositoryReturnsMultipleAllergiesReturnsAllItems()
    {
        var allergies = new List<Allergy>
        {
            new() { Id = 1, AllergyName = "Peanuts", AllergyType = "Food" },
            new() { Id = 2, AllergyName = "Penicillin", AllergyType = "Drug" },
        };
        _repository.Setup(r => r.GetAllergiesAsync()).ReturnsAsync(allergies);

        var result = await _sut.GetAllergiesAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncDelegatesCallToRepository()
    {
        _repository.Setup(r => r.GetAllergiesAsync()).ReturnsAsync([]);

        await _sut.GetAllergiesAsync();

        _repository.Verify(r => r.GetAllergiesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetAllergiesAsyncReturnsExactAllergyInstancesFromRepository()
    {
        var allergy = new Allergy { Id = 5, AllergyName = "Latex", AllergyType = "Contact" };
        _repository.Setup(r => r.GetAllergiesAsync()).ReturnsAsync([allergy]);

        var result = await _sut.GetAllergiesAsync();

        Assert.AreSame(allergy, result[0]);
    }
}
