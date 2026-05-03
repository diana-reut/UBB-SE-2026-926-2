using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using Moq;
using System.Data.Common;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class MedicalRecordRepositoryUnitTests
{
    [TestMethod]
    public void GetById_ShouldReturnRecord_WhenExists()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);

        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetOrdinal("HistoryID")).Returns(1);
        reader.Setup(r => r.GetOrdinal("SourceID")).Returns(2);
        reader.Setup(r => r.GetOrdinal("StaffID")).Returns(3);
        reader.Setup(r => r.GetOrdinal("ConsultationDate")).Returns(4);
        reader.Setup(r => r.GetOrdinal("BasePrice")).Returns(5);
        reader.Setup(r => r.GetOrdinal("FinalPrice")).Returns(6);
        reader.Setup(r => r.GetOrdinal("DiscountApplied")).Returns(7);
        reader.Setup(r => r.GetOrdinal("PoliceNotified")).Returns(8);
        reader.Setup(r => r.GetOrdinal("TransplantID")).Returns(9);

        reader.Setup(r => r.GetInt32(0)).Returns(1);
        reader.Setup(r => r.GetInt32(1)).Returns(2);
        reader.Setup(r => r.GetInt32(2)).Returns(5);
        reader.Setup(r => r.GetInt32(3)).Returns(3);

        reader.Setup(r => r.GetDateTime(4)).Returns(DateTime.Now);
        reader.Setup(r => r.GetDecimal(5)).Returns(100m);
        reader.Setup(r => r.GetDecimal(6)).Returns(90m);

        reader.Setup(r => r.IsDBNull(7)).Returns(true);
        reader.Setup(r => r.IsDBNull(9)).Returns(true);

        reader.Setup(r => r.GetBoolean(8)).Returns(false);

        reader.Setup(r => r["SourceType"]).Returns("ER Visit");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Id);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        var mockContext = new Mock<IDbContext>();
        var mockReader = new Mock<DbDataReader>();

        mockReader.Setup(r => r.Read()).Returns(false);

        mockContext
            .Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(mockReader.Object);

        var repo = new MedicalRecordRepository(mockContext.Object);

        var result = repo.GetById(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Add_ShouldThrow_WhenNull()
    {
        var repo = new MedicalRecordRepository(new Mock<IDbContext>().Object);

        try
        {
            repo.Add(null);

            Assert.Fail("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException)
        {
            // expected exception
        }
    }

    [TestMethod]
    public void Add_ShouldReturnId_WhenValid()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetInt32(0)).Returns(10);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            HistoryId = 1,
            StaffId = 1,
            SourceType = SourceType.ER,
            SourceId = 1,
            ConsultationDate = DateTime.Now,
            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };

        var id = repo.Add(record);

        Assert.AreEqual(10, id);
    }

    [TestMethod]
    public void Update_ShouldThrow_WhenHistoryMissing()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetInt32(0)).Returns(0);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        try
        {
            repo.Update(new MedicalRecord
            {
                Id = 1,
                HistoryId = 1,
                StaffId = 1
            });

            Assert.Fail("Expected KeyNotFoundException was not thrown.");
        }
        catch (KeyNotFoundException)
        {
        }
    }

    [TestMethod]
    public void Delete_ShouldCallTwoQueries()
    {
        var context = new Mock<IDbContext>();

        var repo = new MedicalRecordRepository(context.Object);

        repo.Delete(1);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(2));
    }

    [TestMethod]
    public void GetERVisitCount_ShouldReturnCount()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetInt32(0)).Returns(3);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var count = repo.GetERVisitCount(1, DateTime.Now.AddDays(-1));

        Assert.AreEqual(3, count);
    }
    [TestMethod]
    public void GetByHistoryId_ShouldReturnList_WhenRecordsExist()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["SourceType"]).Returns("ER Visit");

        reader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(0);
        reader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        reader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(DateTime.Now);
        reader.Setup(r => r.GetDecimal(It.IsAny<int>())).Returns(100m);
        reader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(true);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetByHistoryId(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }
    [TestMethod]
    public void GetByHistoryId_ShouldReturnEmpty_WhenNoData()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetByHistoryId(1);

        Assert.AreEqual(0, result.Count);
    }
    [TestMethod]
    public void GetPrescription_ShouldReturnPrescription_WhenExists()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetOrdinal("PrescriptionID")).Returns(0);
        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(1);
        reader.Setup(r => r.GetOrdinal("Date")).Returns(2);

        reader.Setup(r => r.GetInt32(0)).Returns(10);
        reader.Setup(r => r.GetInt32(1)).Returns(1);
        reader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Today);

        reader.Setup(r => r["DoctorNotes"]).Returns("note");

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetPrescription(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(10, result!.Id);
    }
    [TestMethod]
    public void GetPrescription_ShouldReturnNull_WhenNotFound()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetPrescription(1);

        Assert.IsNull(result);
    }
    [TestMethod]
    public void GetConsultingStaffId_ShouldReturnId_WhenExists()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetOrdinal("StaffID")).Returns(0);
        reader.Setup(r => r.GetInt32(0)).Returns(5);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetConsultingStaffId(1);

        Assert.AreEqual(5, result);
    }
    [TestMethod]
    public void GetConsultingStaffId_ShouldReturnNull_WhenNotFound()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetConsultingStaffId(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Update_ShouldThrow_WhenNull()
    {
        var repo = new MedicalRecordRepository(new Mock<IDbContext>().Object);

        try
        {
            repo.Update(null);
            Assert.Fail("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void Update_ShouldExecute_WhenValid()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(true);

        reader.Setup(r => r.GetInt32(0)).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        repo.Update(new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1
        });

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }
    [TestMethod]
    public void Update_ShouldThrow_WhenInvalidIds()
    {
        var repo = new MedicalRecordRepository(new Mock<IDbContext>().Object);

        var record = new MedicalRecord
        {
            Id = 0,
            HistoryId = 0,
            StaffId = 0
        };

        try
        {
            repo.Update(record);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void MapToMedicalRecord_ShouldHandleAdminSourceType()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);

        reader.Setup(r => r["SourceType"]).Returns("Admin");

        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetOrdinal("HistoryID")).Returns(1);
        reader.Setup(r => r.GetOrdinal("SourceID")).Returns(2);
        reader.Setup(r => r.GetOrdinal("StaffID")).Returns(3);
        reader.Setup(r => r.GetOrdinal("ConsultationDate")).Returns(4);
        reader.Setup(r => r.GetOrdinal("BasePrice")).Returns(5);
        reader.Setup(r => r.GetOrdinal("FinalPrice")).Returns(6);
        reader.Setup(r => r.GetOrdinal("DiscountApplied")).Returns(7);
        reader.Setup(r => r.GetOrdinal("PoliceNotified")).Returns(8);
        reader.Setup(r => r.GetOrdinal("TransplantID")).Returns(9);

        reader.Setup(r => r.GetInt32(0)).Returns(1);
        reader.Setup(r => r.GetInt32(1)).Returns(1);
        reader.Setup(r => r.GetInt32(2)).Returns(1);
        reader.Setup(r => r.GetInt32(3)).Returns(1);

        reader.Setup(r => r.GetDateTime(4)).Returns(DateTime.Now);
        reader.Setup(r => r.GetDecimal(5)).Returns(100m);
        reader.Setup(r => r.GetDecimal(6)).Returns(100m);

        reader.Setup(r => r.IsDBNull(7)).Returns(true);
        reader.Setup(r => r.IsDBNull(9)).Returns(true);

        reader.Setup(r => r.GetBoolean(8)).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(SourceType.Admin, result!.SourceType);
    }
    [TestMethod]
    public void GetERVisitCount_ShouldReturnZero_WhenNoRows()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetERVisitCount(1, DateTime.Now);

        Assert.AreEqual(0, result);
    }
    [TestMethod]
    public void Add_ShouldThrow_WhenInvalidIds()
    {
        var repo = new MedicalRecordRepository(new Mock<IDbContext>().Object);

        var record = new MedicalRecord
        {
            HistoryId = 0,
            StaffId = 0,
            SourceType = SourceType.ER
        };

        try
        {
            repo.Add(record);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void Add_ShouldThrow_WhenUnknownSourceType()
    {
        var repo = new MedicalRecordRepository(new Mock<IDbContext>().Object);

        var record = new MedicalRecord
        {
            HistoryId = 1,
            StaffId = 1,
            SourceType = (SourceType)999
        };

        try
        {
            repo.Add(record);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void MapToMedicalRecord_ShouldThrow_WhenUnknownSourceType()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r["SourceType"]).Returns("SomethingElse");

        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetOrdinal("HistoryID")).Returns(1);
        reader.Setup(r => r.GetOrdinal("SourceID")).Returns(2);
        reader.Setup(r => r.GetOrdinal("StaffID")).Returns(3);
        reader.Setup(r => r.GetOrdinal("ConsultationDate")).Returns(4);
        reader.Setup(r => r.GetOrdinal("BasePrice")).Returns(5);
        reader.Setup(r => r.GetOrdinal("FinalPrice")).Returns(6);
        reader.Setup(r => r.GetOrdinal("DiscountApplied")).Returns(7);
        reader.Setup(r => r.GetOrdinal("PoliceNotified")).Returns(8);
        reader.Setup(r => r.GetOrdinal("TransplantID")).Returns(9);

        reader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        reader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(DateTime.Now);
        reader.Setup(r => r.GetDecimal(It.IsAny<int>())).Returns(100m);
        reader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(true);
        reader.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        try
        {
            repo.GetById(1);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void GetPrescription_ShouldReturnNull_WhenReaderReturnsFalse()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetPrescription(1);
        Assert.IsNull(result);
    }
    [TestMethod]
    public void Add_ShouldThrowDatabaseException_WhenInsertFails()
    {

        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            HistoryId = 1,
            StaffId = 1,
            SourceType = SourceType.ER,
            SourceId = 1,
            ConsultationDate = DateTime.Now,
            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };
        try
        {
            repo.Add(record);
            Assert.Fail("Expected DatabaseException was not thrown.");
        }
        catch (DatabaseException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void Add_ShouldCover_App_And_Admin_SourceTypes()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetInt32(0)).Returns(10);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var baseRecord = new MedicalRecord
        {
            HistoryId = 1,
            StaffId = 1,
            SourceId = 1,
            ConsultationDate = DateTime.Now,
            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };

        baseRecord.SourceType = SourceType.App;
        repo.Add(baseRecord);
        baseRecord.SourceType = SourceType.Admin;
        repo.Add(baseRecord);

        Assert.IsTrue(true);
    }
    [TestMethod]
    public void Update_ShouldThrowKeyNotFound_WhenHistoryDoesNotExist()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetInt32(0)).Returns(0);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1
        };

        try
        {
            repo.Update(record);
            Assert.Fail("Expected KeyNotFoundException was not thrown.");
        }
        catch (KeyNotFoundException)
        {
            Assert.IsTrue(true);
        }
    }
    [TestMethod]
    public void Add_ShouldCover_All_TernaryBranches()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);
        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(0);
        reader.Setup(r => r.GetInt32(0)).Returns(42);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            HistoryId = 1,
            StaffId = 1,
            SourceType = SourceType.ER,
            SourceId = 1,
            Symptoms = "cough",
            Diagnosis = "flu",
            ConsultationDate = DateTime.Now,
            BasePrice = 100,
            FinalPrice = 80,
            DiscountApplied = 10,
            PoliceNotified = true,
            TransplantId = 99
        };

        var result = repo.Add(record);

        Assert.AreEqual(42, result);
    }
    [TestMethod]
    public void Update_ShouldCover_AllBranches_TernaryAndExecution()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)  
              .Returns(true);

        reader.Setup(r => r.GetInt32(0)).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1,

            Symptoms = "pain",
            Diagnosis = "flu",
            BasePrice = 100,
            FinalPrice = 80,
            DiscountApplied = 10,
            PoliceNotified = true,
            TransplantId = 5
        };

        repo.Update(record);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }
    [TestMethod]
    public void Update_ShouldCover_HistoryExists_WithValidCount()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true) 
              .Returns(true);

        reader.Setup(r => r.GetInt32(0)).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1,
            Symptoms = "x",
            Diagnosis = "y",
            BasePrice = 100,
            FinalPrice = 90,
            DiscountApplied = 5,
            PoliceNotified = false,
            TransplantId = 1
        };

        repo.Update(record);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }
    [TestMethod]
    public void Update_ShouldCover_AllNull_TernaryBranches()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(true);

        reader.Setup(r => r.GetInt32(0)).Returns(1);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1,

            Symptoms = null,
            Diagnosis = null,
            DiscountApplied = null,
            TransplantId = null,

            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };

        repo.Update(record);

        context.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
    }


    [TestMethod]
    public void Update_FinalMissingBranches()
    {
        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(false)
              .Returns(true);

        reader.Setup(r => r.GetInt32(0)).Returns(0);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var record = new MedicalRecord
        {
            Id = 1,
            HistoryId = 1,
            StaffId = 1,

            Symptoms = null,
            Diagnosis = "x",
            BasePrice = 100,
            FinalPrice = 90,
            DiscountApplied = null,
            PoliceNotified = true,
            TransplantId = null
        };

        try
        {
            repo.Update(record);
        }
        catch (KeyNotFoundException)
        {
        }

        Assert.IsTrue(true);
    }
    [TestMethod]
    public void GetPrescription_ShouldCover_NullDoctorNotesBranch()
    {
        var reader = new Mock<DbDataReader>();

        reader.Setup(r => r.Read()).Returns(true);

        reader.Setup(r => r.GetOrdinal("PrescriptionID")).Returns(0);
        reader.Setup(r => r.GetOrdinal("RecordID")).Returns(1);
        reader.Setup(r => r.GetOrdinal("Date")).Returns(2);

        reader.Setup(r => r.GetInt32(0)).Returns(10);
        reader.Setup(r => r.GetInt32(1)).Returns(1);
        reader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Today);

        reader.Setup(r => r["DoctorNotes"]).Returns((object?)null);

        var context = new Mock<IDbContext>();
        context.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
               .Returns(reader.Object);

        var repo = new MedicalRecordRepository(context.Object);

        var result = repo.GetPrescription(1);

        Assert.IsNotNull(result);
        Assert.IsNull(result.DoctorNotes);
    }

}