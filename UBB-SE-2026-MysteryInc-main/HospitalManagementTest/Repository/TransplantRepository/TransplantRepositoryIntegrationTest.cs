using HospitalManagement.Configuration;
using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using System.Text.Json;

namespace HospitalManagement.Tests.IntegrationTests;

[TestClass]
public class TransplantRepositoryIntegrationTests
{
    private IDbContext _context = null!;
    private ITransplantRepository _repo = null!;

    private readonly List<int> _insertedIds = [];

    public TestContext TestContext { get; set; } = null!;

    private const int SeedReceiverId = 1;
    private const int SeedDonorId = 2;
    private const string SeedOrganType = "Kidney";

    [TestInitialize]
    public void Setup()
    {
        string filePath = Path.Combine(
            AppContext.BaseDirectory, "configuration", "testconfig.local.json");

        TestContext.WriteLine($"Config path: {filePath}");

        string json = File.ReadAllText(filePath);
        string connStr = JsonSerializer.Deserialize<JsonElement>(json)
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()!;

        typeof(Config)
            .GetProperty("ConnectionString")!
            .SetValue(null, connStr);

        _context = new HospitalDbContext();
        _repo = new TransplantRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_insertedIds.Count > 0)
        {
            string ids = string.Join(",", _insertedIds);
            _context.ExecuteNonQuery(
                $"DELETE FROM Transplants WHERE TransplantID IN ({ids})");
        }

        (_context as IDisposable)?.Dispose();
    }

    private int InsertAndTrack(
        int receiverId = SeedReceiverId,
        string organType = SeedOrganType,
        string status = "Pending",
        int? donorId = null,
        float compatibilityScore = 0f)
    {
        string donorSql = donorId.HasValue ? donorId.Value.ToString() : "NULL";

        using var reader = _context.ExecuteQuery($@"
            INSERT INTO Transplants
                (ReceiverID, DonorID, OrganType, RequestDate, TransplantDate, Status, CompatibilityScore)
            OUTPUT INSERTED.TransplantID
            VALUES (
                {receiverId},
                {donorSql},
                '{organType}',
                GETDATE(),
                NULL,
                '{status}',
                {compatibilityScore.ToString(System.Globalization.CultureInfo.InvariantCulture)}
            )");

        reader.Read();
        int id = (int)reader["TransplantID"];
        _insertedIds.Add(id);
        return id;
    }

    [TestMethod]
    public void Add_ShouldInsertTransplant_WithPendingStatus()
    {
        var transplant = new Transplant
        {
            ReceiverId = SeedReceiverId,
            OrganType = SeedOrganType,
        };

        _repo.Add(transplant);

        int insertedId;
        using (var reader = _context.ExecuteQuery(
            $"SELECT MAX(TransplantID) FROM Transplants WHERE ReceiverID = {SeedReceiverId}"))
        {
            reader.Read();
            insertedId = (int)reader[0];
        }

        _insertedIds.Add(insertedId);

        var inserted = _repo.GetById(insertedId);
        Assert.IsNotNull(inserted);
        Assert.AreEqual(TransplantStatus.Pending, inserted!.Status);
        Assert.IsNull(inserted.DonorId);
        Assert.IsNull(inserted.TransplantDate);
        Assert.AreEqual(0f, inserted.CompatibilityScore, delta: 0.001f);
    }

        [TestMethod]
    public void Add_ShouldThrowArgumentNullException_WhenTransplantIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _repo.Add(null!));
    }

    [TestMethod]
    public void GetById_ShouldReturnTransplant_WhenExists()
    {
        int id = InsertAndTrack();

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(id, result!.TransplantId);
        Assert.AreEqual(SeedReceiverId, result.ReceiverId);
        Assert.AreEqual(SeedOrganType, result.OrganType);
        Assert.AreEqual(TransplantStatus.Pending, result.Status);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        var result = _repo.GetById(int.MaxValue);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_ShouldMapDonorId_AsNull_WhenNotAssigned()
    {
        int id = InsertAndTrack(donorId: null);

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.IsNull(result!.DonorId);
    }

    [TestMethod]
    public void GetById_ShouldMapTransplantDate_AsNull_WhenNotSet()
    {
        int id = InsertAndTrack();

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.IsNull(result!.TransplantDate);
    }

    [TestMethod]
    public void GetById_ShouldMapCompatibilityScore_Correctly()
    {
        const float expectedScore = 0.88f;
        int id = InsertAndTrack(compatibilityScore: expectedScore);

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedScore, result!.CompatibilityScore, delta: 0.001f);
    }

    [TestMethod]
    [DataRow("Pending", TransplantStatus.Pending)]
    [DataRow("Matched", TransplantStatus.Matched)]
    [DataRow("Scheduled", TransplantStatus.Scheduled)]
    [DataRow("Completed", TransplantStatus.Completed)]
    [DataRow("Cancelled", TransplantStatus.Cancelled)]
    public void GetBy_IdShouldParseAllTransplantStatuses(string statusString, Object expected)
    {
        int id = InsertAndTrack(status: statusString);

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual((TransplantStatus)expected, result!.Status);
    }

    [TestMethod]
    public void GetByReceiverId_ShouldReturnAllTransplants_ForReceiver()
    {
        InsertAndTrack(receiverId: SeedReceiverId, organType: "Kidney");
        InsertAndTrack(receiverId: SeedReceiverId, organType: "Liver");

        var results = _repo.GetByReceiverId(SeedReceiverId);

        Assert.IsGreaterThanOrEqualTo(2, results.Count);
        Assert.IsTrue(results.All(t => t.ReceiverId == SeedReceiverId));
    }

    [TestMethod]
    public void GetByReceiverId_ShouldReturnEmptyList_WhenNoTransplants()
    {
        var results = _repo.GetByReceiverId(int.MaxValue);

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void GetByDonorId_ShouldReturnTransplants_ForDonor()
    {
        InsertAndTrack(donorId: SeedDonorId, status: "Scheduled");

        var results = _repo.GetByDonorId(SeedDonorId);

        Assert.IsGreaterThanOrEqualTo(1, results.Count);
        Assert.IsTrue(results.All(t => t.DonorId == SeedDonorId));
    }

    [TestMethod]
    public void GetByDonorId_ShouldReturnEmptyList_WhenNoDonorMatch()
    {
        var results = _repo.GetByDonorId(int.MaxValue);

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void GetWaitingByOrgan_ShouldReturnOnlyPendingTransplants_ForOrgan()
    {
        InsertAndTrack(organType: SeedOrganType, status: "Pending");
        InsertAndTrack(organType: SeedOrganType, status: "Scheduled");

        var results = _repo.GetWaitingByOrgan(SeedOrganType);

        Assert.IsGreaterThanOrEqualTo(1, results.Count);
        Assert.IsTrue(results.All(t => t.Status == TransplantStatus.Pending));
        Assert.IsTrue(results.All(t => t.OrganType == SeedOrganType));
    }

    [TestMethod]
    public void GetWaitingByOrgan_ShouldReturnEmptyList_WhenNoPendingMatch()
    {
        var results = _repo.GetWaitingByOrgan("NonExistentOrgan_XYZ");

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void GetTopMatches_ShouldReturnAtMostFiveRows()
    {
        for (int i = 0; i < 6; i++)
        {
            InsertAndTrack(organType: "Heart", status: "Pending", compatibilityScore: i * 0.1f);
        }

        var results = _repo.GetTopMatches("Heart");

        Assert.IsLessThanOrEqualTo(5, results.Count);
    }

    [TestMethod]
    public void GetTopMatches_ShouldReturnEmptyList_WhenNoPendingMatch()
    {
        var results = _repo.GetTopMatches("NonExistentOrgan_XYZ");

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void Update_ShouldSetDonorAndScheduledStatus_AndScore()
    {
        int id = InsertAndTrack(status: "Pending");
        const float newScore = 0.91f;

        _repo.Update(id, donorId: SeedDonorId, score: newScore);

        var result = _repo.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(SeedDonorId, result!.DonorId);
        Assert.AreEqual(TransplantStatus.Scheduled, result.Status);
        Assert.AreEqual(newScore, result.CompatibilityScore, delta: 0.001f);
    }
}