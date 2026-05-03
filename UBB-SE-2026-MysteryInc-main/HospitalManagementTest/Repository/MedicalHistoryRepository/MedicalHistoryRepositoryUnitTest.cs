using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using Moq;
using System.Data.Common;
using System.Reflection;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class MedicalHistoryRepositoryUnitTests
{
    [TestMethod]
    public void GetByPatientId_ShouldReturnHistory_WhenExists()
    {
        var mockContext = new Mock<IDbContext>();
        var mockReader = new Mock<DbDataReader>();

        mockReader.Setup(r => r.Read()).Returns(true);
        mockReader.Setup(r => r["HistoryID"]).Returns(1);
        mockReader.Setup(r => r["PatientID"]).Returns(10);
        mockReader.Setup(r => r["BloodType"]).Returns("A");
        mockReader.Setup(r => r["Rh"]).Returns("Positive");

        mockContext
            .Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(mockReader.Object);

        var repo = new MedicalHistoryRepository(mockContext.Object);

        var result = repo.GetByPatientId(10);

        Assert.IsNotNull(result);
        Assert.AreEqual(10, result!.PatientId);
    }

    [TestMethod]
    public void GetByPatientId_ShouldReturnNull_WhenNotFound()
    {
        var mockContext = new Mock<IDbContext>();
        var mockReader = new Mock<DbDataReader>();

        mockReader.Setup(r => r.Read()).Returns(false);

        mockContext
            .Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(mockReader.Object);

        var repo = new MedicalHistoryRepository(mockContext.Object);

        var result = repo.GetByPatientId(10);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_ShouldReturnEntity_WhenFound()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);

        reader.Setup(r => r["HistoryID"]).Returns(1);
        reader.Setup(r => r["PatientID"]).Returns(10);

        reader.Setup(r => r["BloodType"]).Returns(DBNull.Value);
        reader.Setup(r => r["Rh"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Id);
        Assert.AreEqual(10, result.PatientId);
    }

    [TestMethod]
    public void Create_ShouldReturnId_WhenValidHistory()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            BloodType = BloodType.A,
            Rh = Rh.Positive,
            ChronicConditions = new List<string> { "diabetes" }
        };

        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r[0]).Returns("5");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var id = repo.Create(history);

        Assert.AreEqual(5, id);
    }

    [TestMethod]
    public void Create_ShouldHandleNullEnumsAndEmptyConditions()
    {
        var history = new MedicalHistory
        {
            PatientId = 1,
            BloodType = null,
            Rh = null,
            ChronicConditions = new List<string>()
        };

        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r[0]).Returns("1");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var id = repo.Create(history);

        Assert.AreEqual(1, id);
    }

    [TestMethod]
    public void SaveAllergies_ShouldExecuteForEachItem()
    {
        var context = new Mock<IDbContext>();

        var repo = new MedicalHistoryRepository(context.Object);

        var allergies = new List<(Allergy, string)>
    {
        (new Allergy { AllergyId = 1 }, "mild"),
        (new Allergy { AllergyId = 2 }, "severe")
    };

        repo.SaveAllergies(1, allergies);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(2));
    }

    [TestMethod]
    public void GetChronicConditions_ShouldParseCSV()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["ChronicConditions"]).Returns("diabetes, asthma");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetChronicConditions(1);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Update_ShouldExecute_WhenValidHistory()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["PatientID"]).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var history = new MedicalHistory
        {
            Id = 1,
            PatientId = 1,
            BloodType = BloodType.A,
            Rh = Rh.Positive
        };

        repo.Update(history);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }
    [TestMethod]
    public void Update_ShouldThrow_WhenHistoryNotFound()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var history = new MedicalHistory { Id = 999, PatientId = 1 };

        try
        {
            repo.Update(history);
            Assert.Fail("Expected KeyNotFoundException was not thrown.");
        }
        catch (KeyNotFoundException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void Update_ShouldThrow_WhenPatientMismatch()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["PatientID"]).Returns(99);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var history = new MedicalHistory
        {
            Id = 1,
            PatientId = 1
        };

        try
        {
            repo.Update(history);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldReturnMappedData()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Pollen", result[0].Item1.AllergyName);
        Assert.AreEqual("mild", result[0].Item2);
    }
    [TestMethod]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetById(1);

        Assert.IsNull(result);
    }
    [TestMethod]
    public void GetChronicConditions_ShouldReturnEmpty_WhenNull()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["ChronicConditions"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetChronicConditions(1);

        Assert.AreEqual(0, result.Count);
    }
    [TestMethod]
    public void SaveAllergies_ShouldReturn_WhenListIsNull()
    {
        var context = new Mock<IDbContext>();
        var repo = new MedicalHistoryRepository(context.Object);

        repo.SaveAllergies(1, null);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Never);
    }
    [TestMethod]
    public void SaveAllergies_ShouldReturn_WhenListIsEmpty()
    {
        var context = new Mock<IDbContext>();
        var repo = new MedicalHistoryRepository(context.Object);

        repo.SaveAllergies(1, new List<(Allergy, string)>());

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Never);
    }
    [TestMethod]
    public void GetChronicConditions_ShouldReturnEmpty_WhenNoRow()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetChronicConditions(1);

        Assert.AreEqual(0, result.Count);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldReturnEmpty_WhenNoData()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(0, result.Count);
    }
    [TestMethod]
    public void Create_ShouldReturnMinusOne_WhenNoRowReturned()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var history = new MedicalHistory
        {
            PatientId = 1,
            ChronicConditions = new List<string>()
        };

        var result = repo.Create(history);

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldReturnMultipleRows()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(2, result.Count);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleNullSeverityLevel()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].Item2);
    }
    [TestMethod]
    public void Update_ShouldExecute_WithNullEnums()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["PatientID"]).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var history = new MedicalHistory
        {
            Id = 1,
            PatientId = 1,
            BloodType = null,
            Rh = null
        };

        repo.Update(history);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_SingleRow_NormalValues()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleDBNullSeverity()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].Item2);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleNullToString()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");

        reader.Setup(r => r["SeverityLevel"]).Returns(null);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].Item2);
    }

    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleMultipleRows()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns("Pollen");
        reader.Setup(r => r["AllergyType"]).Returns("Respiratory");
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Pollen", result[0].Item1.AllergyName);
        Assert.AreEqual("Pollen", result[1].Item1.AllergyName);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleDBNullFields()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyType"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyCategory"]).Returns(DBNull.Value);
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].Item1.AllergyName);
        Assert.IsNull(result[0].Item1.AllergyType);
        Assert.IsNull(result[0].Item1.AllergyCategory);
        Assert.AreEqual("mild", result[0].Item2);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldHandleNullFields()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);
        reader.Setup(r => r["AllergyName"]).Returns(null);
        reader.Setup(r => r["AllergyType"]).Returns(null);
        reader.Setup(r => r["AllergyCategory"]).Returns(null);
        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("", result[0].Item1.AllergyName);
        Assert.IsNull(result[0].Item1.AllergyType);
        Assert.IsNull(result[0].Item1.AllergyCategory);
        Assert.AreEqual("mild", result[0].Item2);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldCoverAllBranches()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);

        reader.Setup(r => r["AllergyName"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyType"]).Returns(null!);
        reader.Setup(r => r["AllergyCategory"]).Returns("Seasonal");
        reader.Setup(r => r["SeverityLevel"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("", result[0].Item1.AllergyName); 
        Assert.IsNull(result[0].Item1.AllergyType);
        Assert.AreEqual("Seasonal", result[0].Item1.AllergyCategory);
        Assert.AreEqual("", result[0].Item2);
    }
    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldCoverNullFirstOperand()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);

        reader.Setup(r => r["AllergyName"]).Returns(null);
        reader.Setup(r => r["AllergyType"]).Returns(null);
        reader.Setup(r => r["AllergyCategory"]).Returns(null);

        reader.Setup(r => r["SeverityLevel"]).Returns("mild");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetAllergiesByHistoryId_ShouldCoverDBNullBranch()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["AllergyID"]).Returns(1);

        reader.Setup(r => r["AllergyName"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyType"]).Returns(DBNull.Value);
        reader.Setup(r => r["AllergyCategory"]).Returns(DBNull.Value);

        reader.Setup(r => r["SeverityLevel"]).Returns(DBNull.Value);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalHistoryRepository(context.Object);

        var result = repo.GetAllergiesByHistoryId(1);

        Assert.AreEqual(1, result.Count);
    }
}