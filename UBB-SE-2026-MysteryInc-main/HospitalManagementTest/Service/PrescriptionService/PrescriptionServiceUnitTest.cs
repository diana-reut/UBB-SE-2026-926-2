using HospitalManagement;
using HospitalManagement.Entity;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using Moq;

namespace HospitalManagement.Tests.UnitTests;
[TestClass]
public class PrescriptionServiceUnitTest
{
    private Mock<IPrescriptionRepository>? _repositoryMock;
    private PrescriptionService? _prescriptionService;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IPrescriptionRepository>();
        _prescriptionService = new PrescriptionService(_repositoryMock.Object);
    }

    [TestMethod]
    public void GetLatestPrescriptionsShouldReturnDataFromRepository()
    {
        var expected = new List<Prescription>
            {
                new Prescription { Id = 1 },
                new Prescription { Id = 2 }
            };

        _repositoryMock
            .Setup(r => r.GetTopN(2, 1))
            .Returns(expected);

        var result = _prescriptionService.GetLatestPrescriptions(2, 1);
        Assert.AreEqual(expected, result);
        _repositoryMock.Verify(r => r.GetTopN(2, 1), Times.Once);
    }

    [TestMethod]
    public void GetPrescriptionDetailsShouldReturnPrescriptionWhenExists()
    {
        int id = 10;

        var data = new List<Prescription>
            {
                new Prescription { Id = id }
            };

        _repositoryMock
            .Setup(r => r.GetFiltered(It.Is<PrescriptionFilter>(f => f.PrescriptionId == id)))
            .Returns(data);

        var result = _prescriptionService.GetPrescriptionDetails(id);
        Assert.IsNotNull(result);
        Assert.AreEqual(id, result.Id);
    }

    [TestMethod]
    public void GetPrescriptionDetailsShouldThrowWhenNotFound()
    {
        _repositoryMock
            .Setup(r => r.GetFiltered(It.IsAny<PrescriptionFilter>()))
            .Returns(new List<Prescription>());

        try
        {
            _prescriptionService.GetPrescriptionDetails(99);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (ArgumentException ex)
        {
            Assert.AreEqual("Prescription with ID 99 does not exist.", ex.Message);
        }
    }

    [TestMethod]
    public void ApplyFilterShouldReturnTop20WhenFilterIsNull()
    {
        var expected = new List<Prescription>
            {
                new Prescription { Id = 1 }
            };

        _repositoryMock
            .Setup(r => r.GetTopN(20, 1))
            .Returns(expected);

        var result = _prescriptionService.ApplyFilter(null);
        Assert.AreEqual(expected, result);
        _repositoryMock.Verify(r => r.GetTopN(20, 1), Times.Once);
    }

    [TestMethod]
    public void ApplyFilterShouldReturnFilteredResultsWhenValidFilter()
    {
        var filter = new PrescriptionFilter { PrescriptionId = 5 };

        var expected = new List<Prescription>
        {
            new Prescription { Id = 5 }
        };

        _repositoryMock
            .Setup(r => r.GetFiltered(filter))
            .Returns(expected);

        var result = _prescriptionService.ApplyFilter(filter);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ApplyFilterShouldThrowCustomExceptionWhenRepositoryFails()
    {
        var filter = new PrescriptionFilter();

        _repositoryMock
            .Setup(r => r.GetFiltered(filter))
            .Throws(new Exception());

        try
        {
            _prescriptionService.ApplyFilter(filter);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (MyNotImplementedException ex)
        {
            Assert.AreEqual("The medication search could not be completed at this time due to high system load or complex parameters. Please try simplifying your search or try again later.", ex.Message);
        }
    }

    [TestMethod]
    public void ConstructorShouldThrowWhenRepositoryIsNull()
    {
        try
        {
            new PrescriptionService(null);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (ArgumentNullException ex)
        {
            StringAssert.Contains(ex.ParamName, "prescriptionRepository");

        }
    }
}

