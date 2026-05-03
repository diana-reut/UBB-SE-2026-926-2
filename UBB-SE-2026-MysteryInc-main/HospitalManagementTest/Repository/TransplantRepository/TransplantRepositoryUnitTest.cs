using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using Moq;
using System.Data.Common;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class TransplantRepositoryUnitTests
{
    private static Mock<DbDataReader> BuildSingleRowReader(
        int transplantId = 1,
        int receiverId = 10,
        int? donorId = 20,
        string organType = "Kidney",
        DateTime? requestDate = null,
        DateTime? transplantDate = null,
        TransplantStatus status = TransplantStatus.Pending,
        float compatibilityScore = 0.85f)
    {
        requestDate ??= new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var reader = new Mock<DbDataReader>();

        reader.SetupSequence(r => r.Read())
              .Returns(true)
              .Returns(false);

        reader.Setup(r => r["TransplantID"]).Returns(transplantId);
        reader.Setup(r => r["ReceiverID"]).Returns(receiverId);
        reader.Setup(r => r["DonorID"]).Returns(donorId.HasValue ? (object)donorId.Value : DBNull.Value);
        reader.Setup(r => r["OrganType"]).Returns(organType);
        reader.Setup(r => r["RequestDate"]).Returns(requestDate.Value);
        reader.Setup(r => r["TransplantDate"]).Returns(transplantDate.HasValue ? (object)transplantDate.Value : DBNull.Value);
        reader.Setup(r => r["CompatibilityScore"]).Returns(compatibilityScore);

        reader.Setup(r => r.GetOrdinal("Status")).Returns(6);
        reader.Setup(r => r.GetString(6)).Returns(status.ToString());

        return reader;
    }

    private static Mock<DbDataReader> BuildEmptyReader()
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.Read()).Returns(false);
        return reader;
    }

    [TestMethod]
    public void Add_ShouldExecuteNonQuery_WithCorrectSql()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var repo = new TransplantRepository(mockContext.Object);
        var transplant = new Transplant { ReceiverId = 5, OrganType = "Heart" };

        repo.Add(transplant);

        mockContext.Verify(
            c => c.ExecuteNonQuery(It.Is<string>(sql =>
                sql.Contains("INSERT INTO Transplants") &&
                sql.Contains("5") &&
                sql.Contains("'Heart'") &&
                sql.Contains("NULL") &&
                sql.Contains("'Pending'"))),
            Times.Once);
    }

    [TestMethod]
    public void Add_ShouldThrowArgumentNullException_WhenTransplantIsNull()
    {
        var mockContext = new Mock<IDbContext>();
        var repo = new TransplantRepository(mockContext.Object);

        Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
    }

    [TestMethod]
    public void Add_ShouldUseEmptyString_WhenOrganTypeIsNull()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var repo = new TransplantRepository(mockContext.Object);
        var transplant = new Transplant { ReceiverId = 1, OrganType = null! };

        repo.Add(transplant);

        mockContext.Verify(
            c => c.ExecuteNonQuery(It.Is<string>(sql => sql.Contains("''"))),
            Times.Once);
    }

    [TestMethod]
    public void Add_ShouldEscapeSingleQuotesInOrganType()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var repo = new TransplantRepository(mockContext.Object);
        var transplant = new Transplant { ReceiverId = 1, OrganType = "O'Brien Organ" };

        repo.Add(transplant);

        mockContext.Verify(
            c => c.ExecuteNonQuery(It.Is<string>(sql => sql.Contains("O''Brien Organ"))),
            Times.Once);
    }

    [TestMethod]
    public void GetWaitingByOrgan_ShouldReturnPendingTransplants_ForGivenOrgan()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(organType: "Liver", status: TransplantStatus.Pending);

        mockContext.Setup(c => c.ExecuteQuery(It.Is<string>(sql =>
                sql.Contains("'Liver'") && sql.Contains("'Pending'"))))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetWaitingByOrgan("Liver");

        Assert.HasCount(1, result);
        Assert.AreEqual("Liver", result[0].OrganType);
        Assert.AreEqual(TransplantStatus.Pending, result[0].Status);
    }

    [TestMethod]
    public void GetWaitingByOrgan_ShouldReturnEmptyList_WhenNoMatches()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetWaitingByOrgan("Lung");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetWaitingByOrgan_ShouldEscapeSingleQuotesInOrganType()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        repo.GetWaitingByOrgan("O'Brien");

        mockContext.Verify(
            c => c.ExecuteQuery(It.Is<string>(sql => sql.Contains("O''Brien"))),
            Times.Once);
    }

    [TestMethod]
    public void Update_ShouldExecuteNonQuery_WithCorrectParameters()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteNonQuery(It.IsAny<string>())).Returns(1);

        var repo = new TransplantRepository(mockContext.Object);
        repo.Update(id: 3, donorId: 99, score: 0.92f);

        mockContext.Verify(
            c => c.ExecuteNonQuery(It.Is<string>(sql =>
                sql.Contains("DonorID = 99") &&
                sql.Contains("'Scheduled'") &&
                sql.Contains("CompatibilityScore") &&
                sql.Contains("WHERE TransplantID = 3"))),
            Times.Once);
    }

    [TestMethod]
    public void GetTopMatches_ShouldReturnTransplants_OrderedByScore()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(organType: "Kidney", compatibilityScore: 0.95f);

        mockContext.Setup(c => c.ExecuteQuery(It.Is<string>(sql =>
                sql.Contains("TOP 5") &&
                sql.Contains("'Kidney'") &&
                sql.Contains("ORDER BY"))))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetTopMatches("Kidney");

        Assert.HasCount(1, result);
        Assert.AreEqual("Kidney", result[0].OrganType);
    }

    [TestMethod]
    public void GetTopMatches_ShouldReturnEmptyList_WhenNoMatches()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetTopMatches("Heart");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetTopMatches_ShouldEscapeSingleQuotesInOrganType()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        repo.GetTopMatches("O'Brien");

        mockContext.Verify(
            c => c.ExecuteQuery(It.Is<string>(sql => sql.Contains("O''Brien"))),
            Times.Once);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldReturnTransplants_ForGivenReceiver()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(receiverId: 42);

        mockContext.Setup(c => c.ExecuteQuery(It.Is<string>(sql =>
                sql.Contains("ReceiverID = 42"))))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(42);

        Assert.HasCount(1, result);
        Assert.AreEqual(42, result[0].ReceiverId);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldReturnEmptyList_WhenNoTransplants()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(999);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldMapDonorId_AsNull_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(receiverId: 1, donorId: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(1);

        Assert.HasCount(1, result);
        Assert.IsNull(result[0].DonorId);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldMapTransplantDate_WhenNotNull()
    {
        var transplantDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(receiverId: 1, transplantDate: transplantDate);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(transplantDate, result[0].TransplantDate);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldMapTransplantDate_AsNull_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(receiverId: 1, transplantDate: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(1);

        Assert.HasCount(1, result);
        Assert.IsNull(result[0].TransplantDate);
    }

    [TestMethod]
    public void GetByDonorId_ShouldReturnTransplants_ForGivenDonor()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(donorId: 77);

        mockContext.Setup(c => c.ExecuteQuery(It.Is<string>(sql =>
                sql.Contains("DonorID = 77"))))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByDonorId(77);

        Assert.HasCount(1, result);
        Assert.AreEqual(77, result[0].DonorId);
    }

    [TestMethod]
    public void GetByDonorId_ShouldReturnEmptyList_WhenNoTransplants()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByDonorId(999);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetById_ShouldReturnTransplant_WhenExists()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(transplantId: 1);

        mockContext.Setup(c => c.ExecuteQuery(It.Is<string>(sql =>
                sql.Contains("TransplantID = 1"))))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TransplantId);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        var mockContext = new Mock<IDbContext>();
        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(BuildEmptyReader().Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(9999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldMapOrganType_AsEmptyString_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(receiverId: 1, organType: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetByReceiverId(1);

        Assert.HasCount(1, result);
        Assert.AreEqual("", result[0].OrganType);
    }

    [TestMethod]
    public void GetById_ShouldMapDonorId_WhenNotNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(donorId: 55);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(55, result!.DonorId);
    }

    [TestMethod]
    public void GetById_ShouldMapDonorId_AsNull_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(donorId: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.IsNull(result!.DonorId);
    }

    [TestMethod]
    public void GetById_ShouldMapTransplantDate_WhenNotNull()
    {
        var transplantDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(transplantDate: transplantDate);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(transplantDate, result!.TransplantDate);
    }

    [TestMethod]
    public void GetById_ShouldMapTransplantDate_AsNull_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(transplantDate: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.IsNull(result!.TransplantDate);
    }

    [TestMethod]
    public void GetById_ShouldMapOrganType_AsEmptyString_WhenDbNull()
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(organType: null);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual("", result!.OrganType);
    }


    [TestMethod]
    [DataRow(TransplantStatus.Pending)]
    [DataRow(TransplantStatus.Matched)]
    [DataRow(TransplantStatus.Scheduled)]
    [DataRow(TransplantStatus.Completed)]
    [DataRow(TransplantStatus.Cancelled)]
    public void GetById_ShouldParseAllTransplantStatusValues(Object status)
    {
        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(status: (TransplantStatus)status);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(status, result!.Status);
    }

    [TestMethod]
    public void GetById_ShouldMapCompatibilityScore_FromColumnValue_NotColumnIndex()
    {
        const float expectedScore = 0.75f;

        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(compatibilityScore: expectedScore);
        readerMock.Setup(r => r.GetOrdinal("CompatibilityScore")).Returns(7);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(
            expectedScore,
            result!.CompatibilityScore,
            delta: 0.001f,
            message: "CompatibilityScore must come from the column value, not the column ordinal index.");
    }

    [TestMethod]
    public void GetById_BugRegression_ShouldNotReturnColumnIndex_AsCompatibilityScore()
    {
        const float actualScore = 0.75f;
        const int columnIndex = 7;

        var mockContext = new Mock<IDbContext>();
        var readerMock = BuildSingleRowReader(compatibilityScore: actualScore);
        readerMock.Setup(r => r.GetOrdinal("CompatibilityScore")).Returns(columnIndex);

        mockContext.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .Returns(readerMock.Object);

        var repo = new TransplantRepository(mockContext.Object);
        var result = repo.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(
            (float)columnIndex,
            result!.CompatibilityScore,
            message: "CompatibilityScore must NOT equal the column ordinal index. Bug re-introduced.");
    }
}