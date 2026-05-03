using HospitalManagement.Configuration;
using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using System.Text.Json;

namespace HospitalManagement.Tests.IntegrationTests;

[TestClass]
public class PrescriptionRepositoryIntegrationTests
{
    private IDbContext? _context;
    private IPrescriptionRepository? _repo;


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
        _repo = new PrescriptionRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        (_context as IDisposable)?.Dispose();
    }

    [TestMethod]
    public void Setup_ShouldConnectToDatabase()
    {
        var prescriptions = _repo?.GetAll();

        Assert.IsNotNull(prescriptions);
    }


    [TestMethod]
    public void GetAll_ShouldReturnNonNullList()
    {
        var result = _repo?.GetAll();

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAll_ShouldReturnPrescriptionsWithMedicationLists()
    {
        var result = _repo?.GetAll();

        Assert.IsNotNull(result);

        foreach (var prescription in result)
        {
            Assert.IsNotNull(prescription.MedicationList);
        }
    }

    [TestMethod]
    public void GetTopN_ShouldReturnNonNullList()
    {
        var result = _repo?.GetTopN(5, 1);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetTopN_ShouldRespectPageSize()
    {
        const int PageSize = 3;

        var result = _repo?.GetTopN(PageSize, 1);

        Assert.IsNotNull(result);
        Assert.IsLessThanOrEqualTo(PageSize, result.Count);
    }

    [TestMethod]
    public void GetTopN_ShouldNormalizeZeroN()
    {
        var result = _repo?.GetTopN(0, 1);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetTopN_ShouldNormalizeZeroPage()
    {
        var result = _repo?.GetTopN(5, 0);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetByRecordId_ShouldReturnNull_WhenIdIsNegative()
    {
        var result = _repo?.GetByRecordId(-1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByRecordId_ShouldReturnNull_WhenIdIsZero()
    {
        var result = _repo?.GetByRecordId(0);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByRecordId_ShouldReturnNullOrPrescription_WhenIdIsPositive()
    {
        var result = _repo?.GetByRecordId(999999);

        if (result is not null)
            Assert.IsGreaterThan(0, result.Id);
        else
            Assert.IsNull(result);
    }


    [TestMethod]
    public void GetItems_ShouldReturnEmptyList_WhenIdIsZero()
    {
        var result = _repo?.GetItems(0);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetItems_ShouldReturnEmptyList_WhenIdIsNegative()
    {
        var result = _repo?.GetItems(-1);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetItems_ShouldReturnNonNullList_WhenIdIsValid()
    {
        var result = _repo?.GetItems(999999);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetFiltered_ShouldReturnNonNullList_WhenFilterIsEmpty()
    {
        var result = _repo?.GetFiltered(new PrescriptionFilter());

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetFiltered_ShouldReturnNonNullList_WhenFilterIsNull()
    {
        var result = _repo?.GetFiltered(null!);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetFiltered_ShouldReturnEmptyList_WhenNoMatchFound()
    {
        var filter = new PrescriptionFilter
        {
            PrescriptionId = int.MaxValue  
        };

        var result = _repo?.GetFiltered(filter);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetFiltered_ShouldNotThrow_WhenDateRangeApplied()
    {
        var filter = new PrescriptionFilter
        {
            DateFrom = new DateTime(2000, 1, 1),
            DateTo = new DateTime(2000, 1, 2),  
        };

        var result = _repo?.GetFiltered(filter);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldReturnNonNullList()
    {
        var result = _repo?.GetAddictCandidatePatients();

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_ShouldReturnUniquePatients()
    {
        var result = _repo?.GetAddictCandidatePatients();

        Assert.IsNotNull(result);

        var uniqueIds = result.Select(p => p.Id).Distinct().Count();
        Assert.AreEqual(result.Count, uniqueIds, "Duplicate patients found - deduplication failed.");
    }

    [TestMethod]
    public void Add_ThenDelete_ShouldSucceed_WhenValidRecordIdExists()
    {
        var existing = _repo?.GetAll();
        if (existing is null || existing.Count == 0)
        {
            Assert.Inconclusive("No existing prescriptions in DB; cannot obtain a valid RecordID.");
            return;
        }

        var target = existing[0];
        int prescriptionId = target.Id;
        int recordId = target.RecordId;

        _repo!.Delete(prescriptionId);

        var prescription = new Prescription
        {
            RecordId = recordId,
            DoctorNotes = "Integration test - safe to delete",
            Date = DateTime.Today,
            MedicationList =
            [
                new PrescriptionItem { MedName = "TestMed", Quantity = "10mg" },
        ]
        };

        _repo.Add(prescription);

        var allAfterAdd = _repo.GetAll();
        var added = allAfterAdd.FirstOrDefault(p => p.DoctorNotes == "Integration test - safe to delete");

        Assert.IsNotNull(added, "Prescription was not found after Add.");
        Assert.HasCount(1, added.MedicationList);
        Assert.AreEqual("TestMed", added.MedicationList[0].MedName);

        _repo.Delete(added.Id);
        _repo.Add(target);
    }


    [TestMethod]
    public void Update_ShouldPersistChanges_WhenValidPrescriptionProvided()
    {
        var original = _repo!.GetByRecordId(12);
        Assert.IsNotNull(original, "Seeded prescription with RecordID 12 not found.");

        string? originalNotes = original!.DoctorNotes;
        var originalMeds = original.MedicationList;

        original.DoctorNotes = "After update";
        original.MedicationList =
        [
            new PrescriptionItem { MedName = "UpdatedMed", Quantity = "50mg" },
    ];

        _repo.Update(original);

        var updated = _repo.GetByRecordId(12);
        Assert.IsNotNull(updated, "Prescription not found after Update.");
        Assert.AreEqual("After update", updated!.DoctorNotes);
        Assert.HasCount(1, updated.MedicationList);
        Assert.AreEqual("UpdatedMed", updated.MedicationList[0].MedName);

        original.DoctorNotes = originalNotes;
        original.MedicationList = originalMeds;
        _repo.Update(original);
    }

    [TestMethod]
    public void Delete_ShouldSilentlyDoNothing_WhenIdIsInvalid()
    {
        _repo!.Delete(0);
        _repo!.Delete(-1);
    }
}