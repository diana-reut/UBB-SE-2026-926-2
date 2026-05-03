using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using Moq;
using System;
using System.Data.Common;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class PrescriptionRepositoryUnitTests
{
    private Mock<IDbContext> _mockContext = null!;
    private Mock<DbDataReader> _emptyReader = null!;
    private PrescriptionRepository _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockContext = new Mock<IDbContext>();
        _emptyReader = new Mock<DbDataReader>();
        _emptyReader.Setup(r => r.Read()).Returns(false);
        _repo = new PrescriptionRepository(_mockContext.Object);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-5)]
    public void GetByRecordId_ShouldReturnNull_WhenIdIsNotPositive(int id)
    {
        var result = _repo.GetByRecordId(id);

        Assert.IsNull(result);
        _mockContext.Verify(c => c.ExecuteQuery(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void GetByRecordId_ShouldReturnNull_WhenNoRowFound()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetByRecordId(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByRecordId_ShouldReturnPrescription_WhenRowFound()
    {
        var prescReader = new Mock<DbDataReader>();
        int prescReadCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => prescReadCount++ == 0);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(1);
        prescReader.Setup(r => r["RecordID"]).Returns(2);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 1, 1));

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(_emptyReader.Object);

        var result = _repo.GetByRecordId(2);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual(2, result.RecordId);
        Assert.IsNull(result.DoctorNotes);
        Assert.AreEqual(new DateTime(2025, 1, 1), result.Date);
    }

    [TestMethod]
    public void GetByRecordId_ShouldLoadMedicationItems_WhenPrescriptionFound()
    {
        var prescReader = new Mock<DbDataReader>();
        int prescReadCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => prescReadCount++ == 0);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(10);
        prescReader.Setup(r => r["RecordID"]).Returns(3);
        prescReader.Setup(r => r["DoctorNotes"]).Returns("Take after meals");
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 6, 1));

        var itemReader = new Mock<DbDataReader>();
        int itemReadCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => itemReadCount++ == 0);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(100);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(10);
        itemReader.Setup(r => r["MedName"]).Returns("Aspirin");
        itemReader.Setup(r => r["Quantity"]).Returns("100mg");

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(itemReader.Object);

        var result = _repo.GetByRecordId(3);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result.MedicationList);
        Assert.AreEqual("Aspirin", result.MedicationList[0].MedName);
        Assert.AreEqual("Take after meals", result.DoctorNotes);
    }

    [TestMethod]
    public void Add_ShouldThrowArgumentNullException_WhenPrescriptionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _repo.Add(null!));
    }

    [TestMethod]
    public void Add_ShouldBeginTransactionAndCommit_WhenPrescriptionIsValidWithNoItems()
    {
        var idReader = new Mock<DbDataReader>();
        idReader.Setup(r => r.Read()).Returns(true);
        idReader.Setup(r => r[0]).Returns("1");

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(idReader.Object);

        _repo.Add(new Prescription { RecordId = 1, Date = DateTime.Today });

        _mockContext.Verify(c => c.BeginTransaction(), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Once);
        _mockContext.Verify(c => c.RollbackTransaction(), Times.Never);
    }

    [TestMethod]
    public void Add_ShouldInsertMedicationItems_WhenListIsProvided()
    {
        var idReader = new Mock<DbDataReader>();
        idReader.Setup(r => r.Read()).Returns(true);
        idReader.Setup(r => r[0]).Returns("5");

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(idReader.Object);
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>()))
            .Returns(1);

        var prescription = new Prescription
        {
            RecordId = 1,
            Date = DateTime.Today,
            MedicationList =
            [
                new PrescriptionItem { MedName = "Aspirin",   Quantity = "100mg" },
                new PrescriptionItem { MedName = "Ibuprofen", Quantity = "200mg" },
            ]
        };

        _repo.Add(prescription);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(2));
        _mockContext.Verify(c => c.CommitTransaction(), Times.Once);
    }

    [TestMethod]
    public void Add_ShouldHandleNullQuantity_InMedicationItems()
    {
        var idReader = new Mock<DbDataReader>();
        idReader.Setup(r => r.Read()).Returns(true);
        idReader.Setup(r => r[0]).Returns("1");
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>())).Returns(idReader.Object);

        var prescription = new Prescription
        {
            RecordId = 1,
            MedicationList = new List<PrescriptionItem>
            {
                new PrescriptionItem { MedName = "Placebo", Quantity = null }
            }
        };

        _repo.Add(prescription);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.Is<string>(sql =>
            sql.Contains("NULL") || sql.Contains("null"))), Times.Once);
    }

    [TestMethod]
    public void Add_ShouldRollback_WhenExecuteQueryThrows()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Throws(new Exception("DB connection lost"));

        Assert.Throws<Exception>(() =>
            _repo.Add(new Prescription { RecordId = 1 }));

        _mockContext.Verify(c => c.RollbackTransaction(), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Never);
    }

    [TestMethod]
    public void Add_ShouldRollbackAndThrow_WhenInsertedIdIsNotRetrieved()
    {
        var idReader = new Mock<DbDataReader>();
        idReader.Setup(r => r.Read()).Returns(false);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(idReader.Object);

        Assert.Throws<Exception>(() =>
            _repo.Add(new Prescription { RecordId = 1 }));

        _mockContext.Verify(c => c.RollbackTransaction(), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Never);
    }

    [TestMethod]
    public void Add_ShouldUseDateFromPrescription_WhenDateIsSet()
    {
        var idReader = new Mock<DbDataReader>();
        idReader.Setup(r => r.Read()).Returns(true);
        idReader.Setup(r => r[0]).Returns("1");

        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(idReader.Object);

        _repo.Add(new Prescription { RecordId = 1, Date = new DateTime(2025, 6, 15) });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "2025-06-15");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-10)]
    public void Delete_ShouldDoNothing_WhenIdIsNotPositive(int id)
    {
        _repo.Delete(id);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Never);
        _mockContext.Verify(c => c.BeginTransaction(), Times.Never);
    }

    [TestMethod]
    public void Delete_ShouldDeleteItemsThenPrescriptionAndCommit_WhenIdIsValid()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        _repo.Delete(1);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(2));
        _mockContext.Verify(c => c.CommitTransaction(), Times.Once);
        _mockContext.Verify(c => c.RollbackTransaction(), Times.Never);
    }

    [TestMethod]
    public void Delete_ShouldRollback_WhenExecuteNonQueryThrows()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>()))
            .Throws(new Exception("DB error"));

        Assert.Throws<Exception>(() => _repo.Delete(1));

        _mockContext.Verify(c => c.RollbackTransaction(), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Never);
    }

    [TestMethod]
    public void Update_ShouldThrowArgumentException_WhenPrescriptionIsNullOrIdInvalid()
    {
        Assert.Throws<ArgumentException>(() => _repo.Update(null!));
        Assert.Throws<ArgumentException>(() => _repo.Update(new Prescription { Id = 0, RecordId = 1 }));
        Assert.Throws<ArgumentException>(() => _repo.Update(new Prescription { Id = -1, RecordId = 1 }));
    }

    [TestMethod]
    public void Update_ShouldCommitAndUseNullInSql_WhenDoctorNotesIsNull()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        _repo.Update(new Prescription { Id = 1, RecordId = 2, DoctorNotes = null, Date = DateTime.Today });

        _mockContext.Verify(c => c.ExecuteNonQuery(It.Is<string>(sql =>
            sql.Contains("DoctorNotes = NULL"))), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Once);
        _mockContext.Verify(c => c.RollbackTransaction(), Times.Never);
    }

    [TestMethod]
    public void Update_ShouldUseStringValue_WhenDoctorNotesIsNotNull()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        _repo.Update(new Prescription { Id = 1, RecordId = 1, DoctorNotes = "Patient is stable", Date = DateTime.Now });

        _mockContext.Verify(c => c.ExecuteNonQuery(It.Is<string>(s => s.Contains("'Patient is stable'"))));
    }

    [TestMethod]
    public void Update_ShouldUseNull_WhenItemQuantityIsNull()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var prescription = new Prescription
        {
            Id = 1,
            RecordId = 2,
            Date = DateTime.Today,
            MedicationList =
            [
                new PrescriptionItem { MedName = "Placebo", Quantity = null }
            ]
        };

        _repo.Update(prescription);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.Is<string>(sql =>
            sql.Contains("INSERT") && sql.Contains("NULL"))), Times.Once);
    }

    [TestMethod]
    public void Update_ShouldUseQuantityValue_WhenItemQuantityIsNotNull()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(1);

        var prescription = new Prescription
        {
            Id = 1,
            RecordId = 2,
            Date = DateTime.Today,
            MedicationList =
            [
                new PrescriptionItem { MedName = "Metformin", Quantity = "500mg" }
            ]
        };

        _repo.Update(prescription);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.Is<string>(sql =>
            sql.Contains("'500mg'"))), Times.Once);
    }


    [TestMethod]
    public void Update_ShouldDeleteAndReinsertItems_WhenMedicationListProvided()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var prescription = new Prescription
        {
            Id = 1,
            RecordId = 2,
            Date = DateTime.Today,
            MedicationList =
            [
                new PrescriptionItem { MedName = "Paracetamol", Quantity = "500mg" },
                new PrescriptionItem { MedName = "Amoxicillin",  Quantity = "250mg" },
            ]
        };

        _repo.Update(prescription);

        _mockContext.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(4));
        _mockContext.Verify(c => c.CommitTransaction(), Times.Once);
    }

    [TestMethod]
    public void Update_ShouldNotInsertItems_WhenMedicationListIsNull()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        _repo.Update(new Prescription { Id = 1, RecordId = 2, Date = DateTime.Today, MedicationList = null! });

        _mockContext.Verify(c => c.ExecuteNonQuery(It.IsAny<string>()), Times.Exactly(2));
    }

    [TestMethod]
    public void Update_ShouldRollback_WhenExecuteNonQueryThrows()
    {
        _mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>()))
            .Throws(new Exception("Constraint violation"));

        Assert.Throws<Exception>(() =>
            _repo.Update(new Prescription { Id = 1, RecordId = 2 }));

        _mockContext.Verify(c => c.RollbackTransaction(), Times.Once);
        _mockContext.Verify(c => c.CommitTransaction(), Times.Never);
    }

    [TestMethod]
    public void GetTopN_ShouldReturnEmptyList_WhenNoRowsExist()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetTopN(10, 1);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetTopN_ShouldFallbackToEmptyString_WhenPatientNameIsNull()
    {
        var prescReader = new Mock<DbDataReader>();
        int readCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(1);
        prescReader.Setup(r => r["RecordID"]).Returns(1);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 1, 1));
        prescReader.Setup(r => r["PatientName"]).Returns((object)null!);

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(_emptyReader.Object);

        var result = _repo.GetTopN(10, 1);

        Assert.HasCount(1, result);
        Assert.AreEqual("", result[0].PatientName);
    }

    [TestMethod]
    public void GetTopN_ShouldNormalizeNToTwenty_WhenNIsZeroOrNegative()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetTopN(0, 1);

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "FETCH NEXT 20 ROWS ONLY");
    }

    [TestMethod]
    public void GetTopN_ShouldNormalizePageToOne_WhenPageIsZeroOrNegative()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetTopN(10, 0);

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "OFFSET 0 ROWS");
    }

    [TestMethod]
    public void GetTopN_ShouldCalculateOffsetAndReturnPrescriptions_WhenPageIsGreaterThanOne()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetTopN(10, 3);

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "OFFSET 20 ROWS");
        StringAssert.Contains(capturedSql, "FETCH NEXT 10 ROWS ONLY");
    }

    [TestMethod]
    public void GetTopN_ShouldReturnPrescriptionsWithMedicationItems_WhenRowsExist()
    {
        var prescReader = new Mock<DbDataReader>();
        int prescReadCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => prescReadCount++ < 1);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(99);
        prescReader.Setup(r => r["RecordID"]).Returns(5);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 3, 1));
        prescReader.Setup(r => r["PatientName"]).Returns("Alice Brown");

        var itemReader = new Mock<DbDataReader>();
        int itemReadCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => itemReadCount++ < 1);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(7);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(99);
        itemReader.Setup(r => r["MedName"]).Returns("Ibuprofen");
        itemReader.Setup(r => r["Quantity"]).Returns("400mg");

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(itemReader.Object);

        var result = _repo.GetTopN(10, 1);

        Assert.HasCount(1, result);
        Assert.AreEqual("Alice Brown", result[0].PatientName);
        Assert.AreEqual(99, result[0].Id);
        Assert.IsNotNull(result[0].MedicationList);
        Assert.HasCount(1, result[0].MedicationList);
        Assert.AreEqual("Ibuprofen", result[0].MedicationList[0].MedName);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void GetItems_ShouldReturnEmptyList_WhenIdIsNotPositive(int id)
    {
        var result = _repo.GetItems(id);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
        _mockContext.Verify(c => c.ExecuteQuery(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void GetItems_ShouldReturnEmptyList_WhenNoRowsExist()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetItems(1);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetItems_ShouldCallEnsureConnectionOpen_AndReturnItems_WhenRowsExist()
    {
        var itemReader = new Mock<DbDataReader>();
        int readCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(10);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(1);
        itemReader.Setup(r => r["MedName"]).Returns("Aspirin");
        itemReader.Setup(r => r["Quantity"]).Returns("100mg");

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(itemReader.Object);

        var result = _repo.GetItems(1);

        _mockContext.Verify(c => c.EnsureConnectionOpen(), Times.Once);
        Assert.HasCount(1, result);
        Assert.AreEqual("Aspirin", result[0].MedName);
        Assert.AreEqual("100mg", result[0].Quantity);
        Assert.AreEqual(10, result[0].PrescrItemId);
    }

    [TestMethod]
    public void GetItems_ShouldSetNullQuantity_WhenQuantityIsDBNull()
    {
        var itemReader = new Mock<DbDataReader>();
        int readCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(10);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(1);
        itemReader.Setup(r => r["MedName"]).Returns("Aspirin");
        itemReader.Setup(r => r["Quantity"]).Returns(DBNull.Value);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(itemReader.Object);

        var result = _repo.GetItems(1);

        Assert.HasCount(1, result);
        Assert.IsNull(result[0].Quantity);
    }

    [TestMethod]
    public void GetItems_ShouldFallbackToEmptyString_WhenMedNameToStringReturnsNull()
    {
        var mockMedName = new Mock<object>();
        mockMedName.Setup(o => o.ToString()).Returns((string?)null);

        var itemReader = new Mock<DbDataReader>();
        int readCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(1);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(1);
        itemReader.Setup(r => r["MedName"]).Returns(mockMedName.Object);
        itemReader.Setup(r => r["Quantity"]).Returns(DBNull.Value);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(itemReader.Object);

        var result = _repo.GetItems(1);

        Assert.HasCount(1, result);
        Assert.AreEqual("", result[0].MedName);
    }

    [TestMethod]
    public void GetItems_ShouldReturnMultipleItems_WhenMultipleRowsExist()
    {
        var itemReader = new Mock<DbDataReader>();
        int readCount = 0;
        itemReader.Setup(r => r.Read()).Returns(() => readCount++ < 3);
        itemReader.Setup(r => r["PrescrItemID"]).Returns(1);
        itemReader.Setup(r => r["PrescriptionID"]).Returns(1);
        itemReader.Setup(r => r["MedName"]).Returns("Med");
        itemReader.Setup(r => r["Quantity"]).Returns("10mg");

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(itemReader.Object);

        var result = _repo.GetItems(1);

        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void GetFiltered_ShouldDelegateToGetTopN_WhenFilterIsNull()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetFiltered(null!);

        Assert.IsNotNull(result);
        _mockContext.Verify(c => c.ExecuteQuery(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public void GetFiltered_ShouldReturnEmptyList_WhenNoRowsMatch()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetFiltered(new PrescriptionFilter { PrescriptionId = 99999 });

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetFiltered_ShouldBuildDoctorInClause_WhenPatientNameMatchesFakeDoctor()
    {
        string doctorName = MockDoctorProvider.FakeDoctors[0].FirstName;

        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { PatientName = doctorName });

        Assert.IsNotNull(capturedSql);
        StringAssert.DoesNotMatch(capturedSql,
            new System.Text.RegularExpressions.Regex(@"mr\.StaffID IN \(\s*\)"));
        Assert.DoesNotContain("1=0", capturedSql, "Expected a real doctor IN clause, not the no-match sentinel.");
    }

    [TestMethod]
    public void GetFiltered_ShouldFallbackToUnknown_WhenPatientNameIsNull()
    {
        var prescReader = new Mock<DbDataReader>();
        int readCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(1);
        prescReader.Setup(r => r["RecordID"]).Returns(1);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 1, 1));
        prescReader.Setup(r => r["PatientName"]).Returns((object)null!);

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(_emptyReader.Object);

        var result = _repo.GetFiltered(new PrescriptionFilter { PatientId = 1 });

        Assert.HasCount(1, result);
        Assert.AreEqual("Unknown", result[0].PatientName);
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludePrescriptionId_WhenFilterHasPrescriptionId()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { PrescriptionId = 42 });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "p.PrescriptionID = 42");
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludePatientId_WhenFilterHasPatientId()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { PatientId = 7 });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "pat.PatientID = 7");
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludeDoctorId_WhenFilterHasDoctorId()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { DoctorId = 5 });

        _mockContext.Verify(c => c.ExecuteQuery(It.Is<string>(sql =>
            sql.Contains("mr.StaffID = 5"))), Times.Once);
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludePatientName_WhenFilterHasPatientName()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { PatientName = "John Smith" });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "John");
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludeMedNameLike_WhenFilterHasMedName()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter { MedName = "Aspirin" });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "pi.MedName LIKE '%Aspirin%'");
    }

    [TestMethod]
    public void GetFiltered_ShouldIncludeDateRange_WhenBothDatesProvided()
    {
        string? capturedSql = null;
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Callback<string>(sql => capturedSql = sql)
            .Returns(_emptyReader.Object);

        _repo.GetFiltered(new PrescriptionFilter
        {
            DateFrom = new DateTime(2025, 1, 1),
            DateTo = new DateTime(2025, 12, 31),
        });

        Assert.IsNotNull(capturedSql);
        StringAssert.Contains(capturedSql, "p.[Date] >= '2025-01-01'");
        StringAssert.Contains(capturedSql, "p.[Date] <= '2025-12-31'");
    }

    [TestMethod]
    public void GetFiltered_ShouldReturnPrescriptions_WhenRowsMatch()
    {
        var prescReader = new Mock<DbDataReader>();
        int readCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(1);
        prescReader.Setup(r => r["RecordID"]).Returns(1);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 1, 1));
        prescReader.Setup(r => r["PatientName"]).Returns("Jane Smith");

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(_emptyReader.Object);

        var result = _repo.GetFiltered(new PrescriptionFilter { PatientId = 1 });

        Assert.HasCount(1, result);
        Assert.AreEqual("Jane Smith", result[0].PatientName);
    }

    [TestMethod]
    public void GetAll_ShouldReturnEmptyList_WhenNoRowsExist()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetAll();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetAll_ShouldCallEnsureConnectionOpenAndReturnPrescriptions_WhenRowsExist()
    {
        var prescReader = new Mock<DbDataReader>();
        int readCount = 0;
        prescReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        prescReader.Setup(r => r["PrescriptionID"]).Returns(1);
        prescReader.Setup(r => r["RecordID"]).Returns(1);
        prescReader.Setup(r => r["DoctorNotes"]).Returns(DBNull.Value);
        prescReader.Setup(r => r["Date"]).Returns(new DateTime(2025, 1, 1));

        _mockContext.SetupSequence(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(prescReader.Object)
            .Returns(_emptyReader.Object);

        var result = _repo.GetAll();

        _mockContext.Verify(c => c.EnsureConnectionOpen(), Times.AtLeastOnce);
        Assert.HasCount(1, result);
        Assert.AreEqual(1, result[0].Id);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldReturnEmptyList_WhenNoRowsExist()
    {
        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(_emptyReader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldFallbackToEmptyString_WhenStringFieldsReturnNull()
    {
        var mockNull = new Mock<object>();
        mockNull.Setup(o => o.ToString()).Returns((string?)null);

        var reader = new Mock<DbDataReader>();
        int readCount = 0;
        reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        reader.Setup(r => r["PatientID"]).Returns(5);
        reader.Setup(r => r["FirstName"]).Returns(mockNull.Object);
        reader.Setup(r => r["LastName"]).Returns(mockNull.Object);
        reader.Setup(r => r["CNP"]).Returns(mockNull.Object);
        reader.Setup(r => r["DateOfBirth"]).Returns(new DateTime(1985, 7, 20));
        reader.Setup(r => r["DateOfDeath"]).Returns(DBNull.Value);
        reader.Setup(r => r["Sex"]).Returns("M");
        reader.Setup(r => r["Phone"]).Returns((object)null!);
        reader.Setup(r => r["EmergencyContact"]).Returns((object)null!);
        reader.Setup(r => r["Archived"]).Returns(false);
        reader.Setup(r => r["IsDonor"]).Returns(false);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(reader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.HasCount(1, result);
        Assert.AreEqual("", result[0].FirstName);
        Assert.AreEqual("", result[0].LastName);
        Assert.AreEqual("", result[0].Cnp);
        Assert.AreEqual("", result[0].PhoneNo);
        Assert.AreEqual("", result[0].EmergencyContact);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldDefaultSexToM_WhenSexToStringReturnsNull()
    {
        var mockSex = new Mock<object>();
        mockSex.Setup(o => o.ToString()).Returns((string?)null);

        var reader = new Mock<DbDataReader>();
        int readCount = 0;
        reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        reader.Setup(r => r["PatientID"]).Returns(9);
        reader.Setup(r => r["FirstName"]).Returns("Test");
        reader.Setup(r => r["LastName"]).Returns("User");
        reader.Setup(r => r["CNP"]).Returns("1234567890123");
        reader.Setup(r => r["DateOfBirth"]).Returns(new DateTime(1980, 1, 1));
        reader.Setup(r => r["DateOfDeath"]).Returns(DBNull.Value);
        reader.Setup(r => r["Sex"]).Returns(mockSex.Object);
        reader.Setup(r => r["Phone"]).Returns("0700000000");
        reader.Setup(r => r["EmergencyContact"]).Returns("");
        reader.Setup(r => r["Archived"]).Returns(false);
        reader.Setup(r => r["IsDonor"]).Returns(false);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(reader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.HasCount(1, result);
        Assert.AreEqual(Entity.Enums.Sex.M, result[0].Sex);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldUseActualValues_WhenPhoneAndContactAreStrings()
    {
        var reader = new Mock<DbDataReader>();
        int readCount = 0;
        reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        reader.Setup(r => r["PatientID"]).Returns(7);
        reader.Setup(r => r["FirstName"]).Returns("Bob");
        reader.Setup(r => r["LastName"]).Returns("Jones");
        reader.Setup(r => r["CNP"]).Returns("1234567890123");
        reader.Setup(r => r["DateOfBirth"]).Returns(new DateTime(1975, 3, 15));
        reader.Setup(r => r["DateOfDeath"]).Returns(DBNull.Value);
        reader.Setup(r => r["Sex"]).Returns("M");
        reader.Setup(r => r["Phone"]).Returns("0722000000");
        reader.Setup(r => r["EmergencyContact"]).Returns("Jane Jones");
        reader.Setup(r => r["Archived"]).Returns(false);
        reader.Setup(r => r["IsDonor"]).Returns(false);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(reader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.HasCount(1, result);
        Assert.AreEqual("0722000000", result[0].PhoneNo);
        Assert.AreEqual("Jane Jones", result[0].EmergencyContact);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldMapAllFields_WhenDateOfDeathIsNotNull()
    {
        var reader = new Mock<DbDataReader>();
        int readCount = 0;
        reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
        reader.Setup(r => r["PatientID"]).Returns(2);
        reader.Setup(r => r["FirstName"]).Returns("Alice");
        reader.Setup(r => r["LastName"]).Returns("Smith");
        reader.Setup(r => r["CNP"]).Returns("2987654321098");
        reader.Setup(r => r["DateOfBirth"]).Returns(new DateTime(1960, 1, 1));
        reader.Setup(r => r["DateOfDeath"]).Returns(new DateTime(2024, 3, 20));
        reader.Setup(r => r["Sex"]).Returns("F");
        reader.Setup(r => r["Phone"]).Returns("0711111111");
        reader.Setup(r => r["EmergencyContact"]).Returns("");
        reader.Setup(r => r["Archived"]).Returns(false);
        reader.Setup(r => r["IsDonor"]).Returns(true);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(reader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.HasCount(1, result);
        Assert.AreEqual("Alice", result[0].FirstName);
        Assert.AreEqual("Smith", result[0].LastName);
        Assert.IsNotNull(result[0].Dod);
        Assert.AreEqual(new DateTime(2024, 3, 20), result[0].Dod);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldDeduplicatePatients_WhenSamePatientAppearsMultipleTimes()
    {
        var reader = new Mock<DbDataReader>();
        int readCount = 0;
        reader.Setup(r => r.Read()).Returns(() => readCount++ < 2);
        reader.Setup(r => r["PatientID"]).Returns(1);
        reader.Setup(r => r["FirstName"]).Returns("John");
        reader.Setup(r => r["LastName"]).Returns("Doe");
        reader.Setup(r => r["CNP"]).Returns("1234567890123");
        reader.Setup(r => r["DateOfBirth"]).Returns(new DateTime(1990, 5, 10));
        reader.Setup(r => r["DateOfDeath"]).Returns(DBNull.Value);
        reader.Setup(r => r["Sex"]).Returns("M");
        reader.Setup(r => r["Phone"]).Returns("0700000000");
        reader.Setup(r => r["EmergencyContact"]).Returns("");
        reader.Setup(r => r["Archived"]).Returns(false);
        reader.Setup(r => r["IsDonor"]).Returns(false);

        _mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(reader.Object);

        var result = _repo.GetAddictCandidatePatients();

        Assert.HasCount(1, result);
    }
}