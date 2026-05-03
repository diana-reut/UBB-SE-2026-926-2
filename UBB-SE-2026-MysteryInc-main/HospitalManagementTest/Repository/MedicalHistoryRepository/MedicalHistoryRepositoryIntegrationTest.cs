using HospitalManagement.Database;
using HospitalManagement.Repository;
using System.Text.Json;
using HospitalManagement.Configuration;

namespace HospitalManagement.Tests.IntegrationTests;

[TestClass]
public class MedicalHistoryRepositoryIntegrationTests
{
    private IDbContext _context;
    private IMedicalHistoryRepository _repo;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "configuration", "testconfig.local.json");
        string json = File.ReadAllText(filePath);

        string connStr = JsonSerializer.Deserialize<JsonElement>(json)
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()!;

        typeof(Config)
            .GetProperty("ConnectionString")!
            .SetValue(null, connStr);

        _context = new HospitalDbContext();
        _repo = new MedicalHistoryRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        (_context as IDisposable)?.Dispose();
    }

    [TestMethod]
    public void GetByPatientId_ShouldReturnResult_WhenDatabaseHasData()
    {
        var result = _repo.GetByPatientId(1);

        Assert.IsNotNull(result);
    }
}