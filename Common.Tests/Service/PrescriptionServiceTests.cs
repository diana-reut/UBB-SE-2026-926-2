using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.API.Service;
using Common.Data;
using Common.Data.Entity;
using Common.Data.Integration;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class PrescriptionServiceTests
{
    private Mock<IPrescriptionRepository> _repository = null!;
    private PrescriptionService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new Mock<IPrescriptionRepository>();
        _sut = new PrescriptionService(_repository.Object);
    }

    private static Prescription MakePrescription(int id = 1) => new()
    {
        Id = id,
        RecordId = 10,
        Date = DateTime.Today,
    };

    [TestMethod]
    public void ConstructorWhenRepositoryIsNullThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PrescriptionService(null!));
    }

    [TestMethod]
    public async Task GetLatestPrescriptionsAsyncDelegatesCallToRepository()
    {
        _repository.Setup(r => r.GetTopNAsync(5, 2)).ReturnsAsync([]);

        await _sut.GetLatestPrescriptionsAsync(5, 2);

        _repository.Verify(r => r.GetTopNAsync(5, 2), Times.Once);
    }

    [TestMethod]
    public async Task GetLatestPrescriptionsAsyncReturnsResultFromRepository()
    {
        var prescriptions = new List<Prescription> { MakePrescription(1), MakePrescription(2) };
        _repository.Setup(r => r.GetTopNAsync(10, 1)).ReturnsAsync(prescriptions);

        var result = await _sut.GetLatestPrescriptionsAsync(10, 1);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetPrescriptionDetailsAsyncWhenFoundReturnsPrescription()
    {
        var prescription = MakePrescription(42);
        _repository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([prescription]);

        var result = await _sut.GetPrescriptionDetailsAsync(42);

        Assert.AreEqual(42, result.Id);
    }

    [TestMethod]
    public async Task GetPrescriptionDetailsAsyncWhenNotFoundThrowsArgumentException()
    {
        _repository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([]);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetPrescriptionDetailsAsync(99));
    }

    [TestMethod]
    public async Task GetPrescriptionDetailsAsyncPassesCorrectIdInFilter()
    {
        _repository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([MakePrescription(7)]);

        await _sut.GetPrescriptionDetailsAsync(7);

        _repository.Verify(r => r.GetFilteredAsync(
            It.Is<PrescriptionFilter>(f => f.PrescriptionId == 7)), Times.Once);
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenFilterIsNullCallsGetTopNAsync()
    {
        _repository.Setup(r => r.GetTopNAsync(20, 1)).ReturnsAsync([]);

        await _sut.ApplyFilterAsync(null!);

        _repository.Verify(r => r.GetTopNAsync(20, 1), Times.Once);
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenFilterIsNullReturnsRepositoryResult()
    {
        var prescriptions = new List<Prescription> { MakePrescription(1) };
        _repository.Setup(r => r.GetTopNAsync(20, 1)).ReturnsAsync(prescriptions);

        var result = await _sut.ApplyFilterAsync(null!);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenFilterIsNotNullCallsGetFilteredAsync()
    {
        var filter = new PrescriptionFilter { MedName = "Aspirin" };
        _repository.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync([]);

        await _sut.ApplyFilterAsync(filter);

        _repository.Verify(r => r.GetFilteredAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenFilterIsNotNullReturnsFilteredResults()
    {
        var filter = new PrescriptionFilter { MedName = "Aspirin" };
        var prescriptions = new List<Prescription> { MakePrescription(3), MakePrescription(4) };
        _repository.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(prescriptions);

        var result = await _sut.ApplyFilterAsync(filter);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenRepositoryThrowsThrowsMyNotImplementedException()
    {
        var filter = new PrescriptionFilter { MedName = "Aspirin" };
        _repository.Setup(r => r.GetFilteredAsync(filter)).ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<MyNotImplementedException>(
            () => _sut.ApplyFilterAsync(filter));
    }

    [TestMethod]
    public async Task ApplyFilterAsyncWhenFilterIsNullDoesNotCallGetFilteredAsync()
    {
        _repository.Setup(r => r.GetTopNAsync(20, 1)).ReturnsAsync([]);

        await _sut.ApplyFilterAsync(null!);

        _repository.Verify(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()), Times.Never);
    }
}
