using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.Common;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class AllergyRepositoryUnitTests
{
    [TestMethod]
    public void GetAllergies_ShouldReturnMappedAllergy_WhenRowExists()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyId"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new AllergyRepository(context.Object);

        var result = repo.GetAllergies().ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].AllergyId);
        Assert.AreEqual("Pollen", result[0].AllergyName);
        Assert.AreEqual("Respiratory", result[0].AllergyType);
        Assert.AreEqual("Seasonal", result[0].AllergyCategory);
    }

    [TestMethod]
    public void GetAllergies_ShouldReturnEmpty_WhenNoRows()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new AllergyRepository(context.Object);

        var result = repo.GetAllergies();

        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void GetAllergies_ShouldHandleDBNullValues()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyId"]).Returns(5);
        reader.Setup(r => r["AllergyName"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyType"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyCategory"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new AllergyRepository(context.Object);

        var result = repo.GetAllergies().First();

        Assert.AreEqual(5, result.AllergyId);
        Assert.AreEqual("", result.AllergyName);
        Assert.AreEqual("", result.AllergyType);
        Assert.AreEqual("", result.AllergyCategory);
    }


    [TestMethod]
    public void GetAllergies_ShouldHandleNullValues()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyId"]).Returns(10);
        reader.Setup(r => r["AllergyName"]).Returns(null);
        reader.Setup(r => r["AllergyType"]).Returns(null);
        reader.Setup(r => r["AllergyCategory"]).Returns(null);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new AllergyRepository(context.Object);

        var result = repo.GetAllergies().First();

        Assert.AreEqual(10, result.AllergyId);
        Assert.AreNotEqual(result.AllergyName, null);
        Assert.IsNull(result.AllergyType);
        Assert.IsNull(result.AllergyCategory);
    }

    [TestMethod]
    public void GetAllergies_ShouldReturnMultipleRows()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyId"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Dust");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Indoor");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new AllergyRepository(context.Object);

        var result = repo.GetAllergies().ToList();

        Assert.AreEqual(2, result.Count);
    }
}