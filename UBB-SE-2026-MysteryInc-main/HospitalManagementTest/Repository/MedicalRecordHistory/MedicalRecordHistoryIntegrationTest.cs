using HospitalManagement.Database;
using HospitalManagement.Repository;
using System.Text.Json;
using HospitalManagement.Configuration;

namespace HospitalManagement.Tests.IntegrationTests;

[TestClass]
public class MedicalRecordRepositoryIntegrationTests
{
    private IDbContext _context;
    private IMedicalRecordRepository _repo;

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
        _repo = new MedicalRecordRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        (_context as IDisposable)?.Dispose();
    }

    [TestMethod]
    public void GetAll_ShouldReturnRecords()
    {
        var result = _repo.GetAll();

        Assert.IsNotNull(result);
    }
}