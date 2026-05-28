using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Repository;

[TestClass]
public sealed class MedicalRecordRepositoryTests
{
    private static EFHospitalDbContext CreateContext()
    {
        DbContextOptions<EFHospitalDbContext> options =
            new DbContextOptionsBuilder<EFHospitalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new EFHospitalDbContext(options);
    }

    private static MedicalRecordRepository CreateRepository(EFHospitalDbContext context)
    {
        return new MedicalRecordRepository(context);
    }

    [TestMethod]
    public async Task AddAsync_WhenRecordIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.AddAsync(null!));

        Assert.AreEqual("record", result.ParamName);
    }

    [TestMethod]
    public async Task AddAsync_WhenRecordIsValid_ReturnsGeneratedId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        int result = await repository.AddAsync(CreateRecord());

        Assert.AreNotEqual(0, result);
    }

    [TestMethod]
    public void Add_WhenRecordIsValid_ReturnsGeneratedId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        int result = repository.Add(CreateRecord());

        Assert.AreNotEqual(0, result);
    }

    [TestMethod]
    public void Add_WhenRecordIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        ArgumentNullException result =
            Throws<ArgumentNullException>(() =>
                repository.Add(null!));

        Assert.AreEqual("record", result.ParamName);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenRecordIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.UpdateAsync(null!));

        Assert.AreEqual("record", result.ParamName);
    }

    [TestMethod]
    public void Update_WhenRecordIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        ArgumentNullException result =
            Throws<ArgumentNullException>(() =>
                repository.Update(null!));

        Assert.AreEqual("record", result.ParamName);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenRecordIsValid_UpdatesStaffId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord existing = CreateRecord(staffId: 10);
        context.MedicalRecords.Add(existing);
        await context.SaveChangesAsync();

        int recordId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalRecord updated = CreateRecord(
            id: recordId,
            historyId: existing.HistoryId,
            staffId: 20);

        await repository.UpdateAsync(updated);

        Assert.AreEqual(20, context.MedicalRecords.Single().StaffId);
    }

    [TestMethod]
    public void Update_WhenRecordIsValid_UpdatesStaffId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord existing = CreateRecord(staffId: 10);
        context.MedicalRecords.Add(existing);
        context.SaveChanges();

        int recordId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalRecord updated = CreateRecord(
            id: recordId,
            historyId: existing.HistoryId,
            staffId: 20);

        repository.Update(updated);

        Assert.AreEqual(20, context.MedicalRecords.Single().StaffId);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRecordExists_RemovesRecord()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord record = CreateRecord();
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(record.Id);

        Assert.AreEqual(0, context.MedicalRecords.Count());
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRecordDoesNotExist_DoesNothing()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        await repository.DeleteAsync(999);

        Assert.AreEqual(0, context.MedicalRecords.Count());
    }

    [TestMethod]
    public void Delete_WhenRecordExists_RemovesRecord()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord record = CreateRecord();
        context.MedicalRecords.Add(record);
        context.SaveChanges();

        repository.Delete(record.Id);

        Assert.AreEqual(0, context.MedicalRecords.Count());
    }

    [TestMethod]
    public void Delete_WhenRecordDoesNotExist_DoesNothing()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        repository.Delete(999);

        Assert.AreEqual(0, context.MedicalRecords.Count());
    }

    [TestMethod]
    public async Task GetAllAsync_WhenNoRecordsExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        List<MedicalRecord> result = await repository.GetAllAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByHistoryIdAsync_WhenNoRecordsExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        List<MedicalRecord> result = await repository.GetByHistoryIdAsync(999);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord? result = await repository.GetByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_WhenRecordDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord? result = repository.GetById(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetConsultingStaffIdAsync_WhenRecordExists_ReturnsStaffId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord record = CreateRecord(staffId: 44);
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();

        int? result = await repository.GetConsultingStaffIdAsync(record.Id);

        Assert.AreEqual(44, result);
    }

    [TestMethod]
    public async Task GetConsultingStaffIdAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        int? result = await repository.GetConsultingStaffIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetConsultingStaffId_WhenRecordExists_ReturnsStaffId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalRecord record = CreateRecord(staffId: 44);
        context.MedicalRecords.Add(record);
        context.SaveChanges();

        int? result = repository.GetConsultingStaffId(record.Id);

        Assert.AreEqual(44, result);
    }

    [TestMethod]
    public async Task GetERVisitCountAsync_WhenMatchingRecordsExist_ReturnsCount()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 10);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: SourceType.ER,
            consultationDate: DateTime.Today));

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: SourceType.ER,
            consultationDate: DateTime.Today));

        await context.SaveChangesAsync();

        int result = await repository.GetERVisitCountAsync(10, DateTime.Today.AddDays(-1));

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task GetERVisitCountAsync_WhenSourceTypeIsNotER_ReturnsZero()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 10);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: GetNonERSourceType(),
            consultationDate: DateTime.Today));

        await context.SaveChangesAsync();

        int result = await repository.GetERVisitCountAsync(10, DateTime.Today.AddDays(-1));

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetERVisitCountAsync_WhenRecordIsBeforeFromDate_ReturnsZero()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 10);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: SourceType.ER,
            consultationDate: DateTime.Today.AddDays(-10)));

        await context.SaveChangesAsync();

        int result = await repository.GetERVisitCountAsync(10, DateTime.Today);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void GetERVisitCount_WhenMatchingRecordsExist_ReturnsCount()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 10);
        context.MedicalHistory.Add(history);
        context.SaveChanges();

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: SourceType.ER,
            consultationDate: DateTime.Today));

        context.MedicalRecords.Add(CreateRecord(
            historyId: history.Id,
            sourceType: SourceType.ER,
            consultationDate: DateTime.Today));

        context.SaveChanges();

        int result = repository.GetERVisitCount(10, DateTime.Today.AddDays(-1));

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task GetPrescriptionAsync_WhenPrescriptionDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        Prescription? result = await repository.GetPrescriptionAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetPrescription_WhenPrescriptionDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        Prescription? result = repository.GetPrescription(999);

        Assert.IsNull(result);
    }

    private static MedicalRecord CreateRecord(
     int id = 0,
     int historyId = 1,
     int staffId = 1,
     SourceType? sourceType = null,
     DateTime? consultationDate = null)
    {
        return new MedicalRecord
        {
            Id = id,
            HistoryId = historyId,
            StaffId = staffId,
            SourceType = sourceType ?? GetNonERSourceType(),
            SourceId = 1,
            Symptoms = "Cough",
            Diagnosis = "Cold",
            ConsultationDate = consultationDate ?? DateTime.Today,
            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };
    }
    private static MedicalHistory CreateHistory(
        int id = 0,
        int patientId = 1,
        BloodType? bloodType = BloodType.A,
        Rh? rh = Rh.Positive)
    {
        return new MedicalHistory
        {
            Id = id,
            PatientId = patientId,
            BloodType = bloodType,
            Rh = rh,
            ChronicConditions = new List<string>()
        };
    }

    private static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new AssertFailedException(
            $"Expected exception of type {typeof(TException).Name} was not thrown.");
    }

    private static TException Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new AssertFailedException(
            $"Expected exception of type {typeof(TException).Name} was not thrown.");
    }

    private static SourceType GetNonERSourceType()
    {
        return Enum.GetValues<SourceType>()
            .First(sourceType => sourceType != SourceType.ER);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenRecordsExist_ReturnsRecords()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory historyOne = CreateHistory(patientId: 1);
        MedicalHistory historyTwo = CreateHistory(patientId: 2);

        context.MedicalHistory.Add(historyOne);
        context.MedicalHistory.Add(historyTwo);
        await context.SaveChangesAsync();

        context.MedicalRecords.Add(CreateRecord(historyId: historyOne.Id));
        context.MedicalRecords.Add(CreateRecord(historyId: historyTwo.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<MedicalRecord> result = await repository.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetAll_WhenRecordsExist_ReturnsRecords()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory historyOne = CreateHistory(patientId: 1);
        MedicalHistory historyTwo = CreateHistory(patientId: 2);

        context.MedicalHistory.Add(historyOne);
        context.MedicalHistory.Add(historyTwo);
        context.SaveChanges();

        context.MedicalRecords.Add(CreateRecord(historyId: historyOne.Id));
        context.MedicalRecords.Add(CreateRecord(historyId: historyTwo.Id));
        context.SaveChanges();

        context.ChangeTracker.Clear();

        List<MedicalRecord> result = repository.GetAll();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByHistoryIdAsync_WhenRecordsExist_ReturnsMatchingRecords()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory historyOne = CreateHistory(patientId: 1);
        MedicalHistory historyTwo = CreateHistory(patientId: 2);

        context.MedicalHistory.Add(historyOne);
        context.MedicalHistory.Add(historyTwo);
        await context.SaveChangesAsync();

        context.MedicalRecords.Add(CreateRecord(historyId: historyOne.Id));
        context.MedicalRecords.Add(CreateRecord(historyId: historyTwo.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<MedicalRecord> result = await repository.GetByHistoryIdAsync(historyOne.Id);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetByHistoryId_WhenRecordsExist_ReturnsMatchingRecords()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory historyOne = CreateHistory(patientId: 1);
        MedicalHistory historyTwo = CreateHistory(patientId: 2);

        context.MedicalHistory.Add(historyOne);
        context.MedicalHistory.Add(historyTwo);
        context.SaveChanges();

        context.MedicalRecords.Add(CreateRecord(historyId: historyOne.Id));
        context.MedicalRecords.Add(CreateRecord(historyId: historyTwo.Id));
        context.SaveChanges();

        context.ChangeTracker.Clear();

        List<MedicalRecord> result = repository.GetByHistoryId(historyOne.Id);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 1);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        MedicalRecord record = CreateRecord(historyId: history.Id);
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();

        int recordId = record.Id;

        context.ChangeTracker.Clear();

        MedicalRecord? result = await repository.GetByIdAsync(recordId);

        Assert.AreEqual(recordId, result!.Id);
    }

    [TestMethod]
    public void GetById_WhenRecordExists_ReturnsRecord()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalRecordRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(patientId: 1);
        context.MedicalHistory.Add(history);
        context.SaveChanges();

        MedicalRecord record = CreateRecord(historyId: history.Id);
        context.MedicalRecords.Add(record);
        context.SaveChanges();

        int recordId = record.Id;

        context.ChangeTracker.Clear();

        MedicalRecord? result = repository.GetById(recordId);

        Assert.AreEqual(recordId, result!.Id);
    }
}