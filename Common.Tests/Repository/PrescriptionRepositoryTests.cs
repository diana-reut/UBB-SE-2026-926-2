using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class PrescriptionRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [TestMethod]
    public async Task AddAsyncWhenPrescriptionIsNullThrowsArgumentNullException()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
    }

    [TestMethod]
    public async Task AddAsyncWhenPrescriptionIsValidPersistsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        await repo.AddAsync(MakePrescription(record.Id));

        Assert.AreEqual(1, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task UpdateAsyncWhenPrescriptionIsNullThrowsArgumentNullException()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
    }

    [TestMethod]
    public async Task UpdateAsyncWhenPrescriptionDoesNotExistThrowsKeyNotFoundException()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        Prescription ghost = MakePrescription(recordId: 1);
        ghost.Id = 999;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(ghost));
    }

    [TestMethod]
    public async Task UpdateAsyncWhenPrescriptionExistsUpdatesDate()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription existing = MakePrescription(record.Id, DateTime.Today.AddDays(-2));
        context.Prescriptions.Add(existing);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Prescription updated = MakePrescription(record.Id, DateTime.Today);
        updated.Id = existing.Id;
        await repo.UpdateAsync(updated);

        Assert.AreEqual(DateTime.Today, context.Prescriptions.Single().Date);
    }

    [TestMethod]
    public async Task UpdateAsyncWhenMedicationListChangesReplacesMedicationItems()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription existing = MakePrescription(record.Id, medName: "OldMed");
        context.Prescriptions.Add(existing);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Prescription updated = MakePrescription(record.Id, medName: "NewMed");
        updated.Id = existing.Id;
        await repo.UpdateAsync(updated);

        Assert.AreEqual("NewMed", context.PrescriptionItems.Single().MedName);
    }

    [TestMethod]
    public async Task DeleteAsyncWhenPrescriptionExistsRemovesPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = MakePrescription(record.Id);
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(prescription.Id);

        Assert.AreEqual(0, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task DeleteAsyncWhenPrescriptionDoesNotExistDoesNothing()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        await repo.DeleteAsync(999);

        Assert.AreEqual(0, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task GetAllAsyncWhenNoPrescriptionsExistReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        List<Prescription> result = await repo.GetAllAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAllAsyncWhenPrescriptionsExistReturnsAll()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord r1 = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord r2 = await SeedMedicalRecordAsync(context, patientId: 2);
        context.Prescriptions.AddRange(MakePrescription(r1.Id), MakePrescription(r2.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsyncWhenNIsOneLimitsResults()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord r1 = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord r2 = await SeedMedicalRecordAsync(context, patientId: 2);
        context.Prescriptions.AddRange(
            MakePrescription(r1.Id, DateTime.Today.AddDays(-1)),
            MakePrescription(r2.Id, DateTime.Today));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetTopNAsync(1, 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsyncWhenNIsZeroDefaultsToPageSizeTwenty()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetTopNAsync(0, 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsyncWhenPageIsZeroDefaultsToPageOne()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetTopNAsync(20, 0);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsyncWhenPageIsTwoSkipsFirstPage()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord r1 = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord r2 = await SeedMedicalRecordAsync(context, patientId: 2);
        context.Prescriptions.AddRange(
            MakePrescription(r1.Id, DateTime.Today),
            MakePrescription(r2.Id, DateTime.Today.AddDays(-1)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetTopNAsync(1, 2);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetItemsAsyncWhenItemsExistReturnsItems()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = MakePrescription(record.Id, medName: "Ibuprofen");
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<PrescriptionItem> result = await repo.GetItemsAsync(prescription.Id);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetItemsAsyncWhenNoItemsExistReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        List<PrescriptionItem> result = await repo.GetItemsAsync(999);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByRecordIdAsyncWhenPrescriptionExistsReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Prescription? result = await repo.GetByRecordIdAsync(record.Id);

        Assert.AreEqual(record.Id, result!.RecordId);
    }

    [TestMethod]
    public async Task GetByRecordIdAsyncWhenPrescriptionDoesNotExistReturnsNull()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        Prescription? result = await repo.GetByRecordIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenFilterIsNullDelegatesToGetTopNAsync()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(null!);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenPrescriptionIdMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = MakePrescription(record.Id);
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { PrescriptionId = prescription.Id });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenPatientIdMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 77);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { PatientId = 77 });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenDoctorIdMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, staffId: 55);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { DoctorId = 55 });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenMedicationNameMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id, medName: "Amoxicillin"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { MedName = "Amox" });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenDateFromMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Today));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { DateFrom = DateTime.Today.AddDays(-1) });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenDateToMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Today));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { DateTo = DateTime.Today.AddDays(1) });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenPatientFirstNameMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, firstName: "Alice", lastName: "Smith");
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { PatientName = "Ali" });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenPatientLastNameMatchesReturnsPrescription()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, firstName: "Bob", lastName: "Johnson");
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(
            new PrescriptionFilter { PatientName = "John" });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsyncWhenPatientIsArchivedReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, isArchived: true);
        context.Prescriptions.Add(MakePrescription(record.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<Prescription> result = await repo.GetFilteredAsync(new PrescriptionFilter());

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsyncWhenPrescriptionInWindowSetsPoliceNotifiedTrue()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 10);
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Now.AddDays(-1)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await repo.MarkPoliceNotifiedAsync(10);

        Assert.IsTrue(context.MedicalRecords.Single().PoliceNotified);
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsyncWhenPrescriptionOutsideWindowLeavesPoliceNotifiedFalse()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 10);
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Now.AddDays(-31)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await repo.MarkPoliceNotifiedAsync(10);

        Assert.IsFalse(context.MedicalRecords.Single().PoliceNotified);
    }

    [TestMethod]
    public async Task GetPoliceNotifiedPatientIdsAsyncWhenPatientIsNotifiedReturnsId()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 10);
        record.PoliceNotified = true;
        await context.SaveChangesAsync();
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Now.AddDays(-1)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<int> result = await repo.GetPoliceNotifiedPatientIdsAsync(new List<int> { 10 });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetPoliceNotifiedPatientIdsAsyncWhenPatientNotNotifiedReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 10);
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Now.AddDays(-1)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<int> result = await repo.GetPoliceNotifiedPatientIdsAsync(new List<int> { 10 });

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPoliceNotifiedPatientIdsAsyncWhenPatientIdsEmptyReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        List<int> result = await repo.GetPoliceNotifiedPatientIdsAsync(new List<int>());

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPoliceNotifiedPatientIdsAsyncWhenPrescriptionOutsideWindowReturnsEmptyList()
    {
        await using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repo = new(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 10);
        record.PoliceNotified = true;
        await context.SaveChangesAsync();
        context.Prescriptions.Add(MakePrescription(record.Id, DateTime.Now.AddDays(-31)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        List<int> result = await repo.GetPoliceNotifiedPatientIdsAsync(new List<int> { 10 });

        Assert.AreEqual(0, result.Count);
    }

    private static async Task<MedicalRecord> SeedMedicalRecordAsync(
        EFHospitalDbContext context,
        int patientId = 1,
        int staffId = 1,
        string firstName = "John",
        string lastName = "Doe",
        bool isArchived = false)
    {
        Patient patient = new()
        {
            Id = patientId,
            FirstName = firstName,
            LastName = lastName,
            IsArchived = isArchived
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        MedicalHistory history = new()
        {
            PatientId = patient.Id,
            BloodType = BloodType.A,
            Rh = Rh.Positive,
            ChronicConditions = new List<string>()
        };
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        MedicalRecord record = new()
        {
            HistoryId = history.Id,
            StaffId = staffId,
            SourceType = Enum.GetValues<SourceType>().First(s => s != SourceType.ER),
            SourceId = 1,
            Symptoms = "Cough",
            Diagnosis = "Cold",
            ConsultationDate = DateTime.Today,
            BasePrice = 100,
            FinalPrice = 100,
            PoliceNotified = false
        };
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();

        return record;
    }

    private static Prescription MakePrescription(
        int recordId,
        DateTime? date = null,
        string medName = "Aspirin") =>
        new()
        {
            RecordId = recordId,
            Date = date ?? DateTime.Today,
            MedicationList = new List<PrescriptionItem> { new PrescriptionItem { MedName = medName } }
        };
}
