using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Repository;

[TestClass]
public sealed class PrescriptionRepositoryTests
{
    private static EFHospitalDbContext CreateContext()
    {
        DbContextOptions<EFHospitalDbContext> options =
            new DbContextOptionsBuilder<EFHospitalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new EFHospitalDbContext(options);
    }

    private static PrescriptionRepository CreateRepository(EFHospitalDbContext context)
    {
        return new PrescriptionRepository(context);
    }

    [TestMethod]
    public async Task AddAsync_WhenPrescriptionIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.AddAsync(null!));

        Assert.AreEqual("prescription", result.ParamName);
    }

    [TestMethod]
    public async Task AddAsync_WhenPrescriptionIsValid_AddsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        Prescription prescription = CreatePrescription(record.Id);

        await repository.AddAsync(prescription);

        Assert.AreEqual(1, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPrescriptionIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.UpdateAsync(null!));

        Assert.AreEqual("prescription", result.ParamName);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPrescriptionDoesNotExist_ThrowsKeyNotFoundException()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        Prescription prescription = CreatePrescription(recordId: 1);
        prescription.Id = 999;

        KeyNotFoundException result =
            await ThrowsAsync<KeyNotFoundException>(() =>
                repository.UpdateAsync(prescription));

        Assert.IsInstanceOfType(result, typeof(KeyNotFoundException));
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPrescriptionExists_UpdatesDate()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription existing = CreatePrescription(record.Id, DateTime.Today.AddDays(-2), "Aspirin");

        context.Prescriptions.Add(existing);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        Prescription updated = CreatePrescription(record.Id, DateTime.Today, "Aspirin");
        updated.Id = existing.Id;

        await repository.UpdateAsync(updated);

        Assert.AreEqual(DateTime.Today, context.Prescriptions.Single().Date);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenMedicationListChanges_ReplacesMedicationItems()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription existing = CreatePrescription(record.Id, DateTime.Today.AddDays(-2), "OldMed");

        context.Prescriptions.Add(existing);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        Prescription updated = CreatePrescription(record.Id, DateTime.Today, "NewMed");
        updated.Id = existing.Id;

        await repository.UpdateAsync(updated);

        Assert.AreEqual("NewMed", context.PrescriptionItems.Single().MedName);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPrescriptionExists_RemovesPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = CreatePrescription(record.Id);

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(prescription.Id);

        Assert.AreEqual(0, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPrescriptionDoesNotExist_DoesNothing()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        await repository.DeleteAsync(999);

        Assert.AreEqual(0, context.Prescriptions.Count());
    }

    [TestMethod]
    public async Task GetAllAsync_WhenPrescriptionsExist_ReturnsPrescriptions()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord recordOne = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord recordTwo = await SeedMedicalRecordAsync(context, patientId: 2);

        context.Prescriptions.Add(CreatePrescription(recordOne.Id));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenNoPrescriptionsExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        List<Prescription> result = await repository.GetAllAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsync_WhenNIsOne_ReturnsOnePrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord recordOne = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord recordTwo = await SeedMedicalRecordAsync(context, patientId: 2);

        context.Prescriptions.Add(CreatePrescription(recordOne.Id, DateTime.Today.AddDays(-1)));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id, DateTime.Today));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetTopNAsync(1, 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsync_WhenNIsZero_UsesDefaultPageSize()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetTopNAsync(0, 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetTopNAsync_WhenPageIsTwo_SkipsFirstPage()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord recordOne = await SeedMedicalRecordAsync(context, patientId: 1);
        MedicalRecord recordTwo = await SeedMedicalRecordAsync(context, patientId: 2);

        context.Prescriptions.Add(CreatePrescription(recordOne.Id, DateTime.Today));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id, DateTime.Today.AddDays(-1)));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetTopNAsync(1, 2);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetItemsAsync_WhenItemsExist_ReturnsItems()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = CreatePrescription(record.Id, DateTime.Today, "Ibuprofen");

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<PrescriptionItem> result = await repository.GetItemsAsync(prescription.Id);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetItemsAsync_WhenItemsDoNotExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        List<PrescriptionItem> result = await repository.GetItemsAsync(999);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByRecordIdAsync_WhenPrescriptionExists_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = CreatePrescription(record.Id);

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        Prescription? result = await repository.GetByRecordIdAsync(record.Id);

        Assert.AreEqual(record.Id, result!.RecordId);
    }

    [TestMethod]
    public async Task GetByRecordIdAsync_WhenPrescriptionDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        Prescription? result = await repository.GetByRecordIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenFilterIsNull_ReturnsTopPrescriptions()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(null!);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenPrescriptionIdMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);
        Prescription prescription = CreatePrescription(record.Id);

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            PrescriptionId = prescription.Id
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenPatientIdMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, patientId: 77);

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            PatientId = 77
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenDoctorIdMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context, staffId: 55);

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            DoctorId = 55
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenMedicationNameMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        context.Prescriptions.Add(CreatePrescription(record.Id, DateTime.Today, "Amoxicillin"));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            MedName = "Amox"
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenDateFromMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        context.Prescriptions.Add(CreatePrescription(record.Id, DateTime.Today));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            DateFrom = DateTime.Today.AddDays(-1)
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenDateToMatches_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(context);

        context.Prescriptions.Add(CreatePrescription(record.Id, DateTime.Today));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            DateTo = DateTime.Today.AddDays(1)
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenPatientNameMatchesFirstName_ReturnsPrescription()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(
            context,
            firstName: "Alice",
            lastName: "Smith");

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter
        {
            PatientName = "Ali"
        });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetFilteredAsync_WhenPatientIsArchived_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        MedicalRecord record = await SeedMedicalRecordAsync(
            context,
            isArchived: true);

        context.Prescriptions.Add(CreatePrescription(record.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Prescription> result = await repository.GetFilteredAsync(new PrescriptionFilter());

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAddictCandidatePatientsAsync_WhenSameMedicationFromThreeDoctors_ReturnsPatient()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        Patient patient = CreatePatient(firstName: "John", lastName: "Doe");
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        MedicalHistory history = CreateHistory(patient.Id);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        MedicalRecord recordOne = CreateRecord(history.Id, staffId: 1);
        MedicalRecord recordTwo = CreateRecord(history.Id, staffId: 2);
        MedicalRecord recordThree = CreateRecord(history.Id, staffId: 3);

        context.MedicalRecords.Add(recordOne);
        context.MedicalRecords.Add(recordTwo);
        context.MedicalRecords.Add(recordThree);
        await context.SaveChangesAsync();

        context.Prescriptions.Add(CreatePrescription(recordOne.Id, DateTime.Now.AddDays(-1), "Morphine"));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id, DateTime.Now.AddDays(-2), "Morphine"));
        context.Prescriptions.Add(CreatePrescription(recordThree.Id, DateTime.Now.AddDays(-3), "Morphine"));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Patient> result = await repository.GetAddictCandidatePatientsAsync();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetAddictCandidatePatientsAsync_WhenOnlyTwoDoctors_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        Patient patient = CreatePatient(firstName: "John", lastName: "Doe");
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        MedicalHistory history = CreateHistory(patient.Id);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        MedicalRecord recordOne = CreateRecord(history.Id, staffId: 1);
        MedicalRecord recordTwo = CreateRecord(history.Id, staffId: 2);

        context.MedicalRecords.Add(recordOne);
        context.MedicalRecords.Add(recordTwo);
        await context.SaveChangesAsync();

        context.Prescriptions.Add(CreatePrescription(recordOne.Id, DateTime.Now.AddDays(-1), "Morphine"));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id, DateTime.Now.AddDays(-2), "Morphine"));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<Patient> result = await repository.GetAddictCandidatePatientsAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAddictCandidatePatients_WhenSameMedicationFromThreeDoctors_ReturnsPatient()
    {
        using EFHospitalDbContext context = CreateContext();
        PrescriptionRepository repository = CreateRepository(context);

        Patient patient = CreatePatient(firstName: "John", lastName: "Doe");
        context.Patients.Add(patient);
        context.SaveChanges();

        MedicalHistory history = CreateHistory(patient.Id);
        context.MedicalHistory.Add(history);
        context.SaveChanges();

        MedicalRecord recordOne = CreateRecord(history.Id, staffId: 1);
        MedicalRecord recordTwo = CreateRecord(history.Id, staffId: 2);
        MedicalRecord recordThree = CreateRecord(history.Id, staffId: 3);

        context.MedicalRecords.Add(recordOne);
        context.MedicalRecords.Add(recordTwo);
        context.MedicalRecords.Add(recordThree);
        context.SaveChanges();

        context.Prescriptions.Add(CreatePrescription(recordOne.Id, DateTime.Now.AddDays(-1), "Morphine"));
        context.Prescriptions.Add(CreatePrescription(recordTwo.Id, DateTime.Now.AddDays(-2), "Morphine"));
        context.Prescriptions.Add(CreatePrescription(recordThree.Id, DateTime.Now.AddDays(-3), "Morphine"));
        context.SaveChanges();

        context.ChangeTracker.Clear();

        List<Patient> result = repository.GetAddictCandidatePatients();

        Assert.AreEqual(1, result.Count);
    }

    private static async Task<MedicalRecord> SeedMedicalRecordAsync(
        EFHospitalDbContext context,
        int patientId = 1,
        int staffId = 1,
        string firstName = "John",
        string lastName = "Doe",
        bool isArchived = false)
    {
        Patient patient = CreatePatient(
            id: patientId,
            firstName: firstName,
            lastName: lastName,
            isArchived: isArchived);

        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        MedicalHistory history = CreateHistory(patient.Id);
        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        MedicalRecord record = CreateRecord(history.Id, staffId);
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();

        return record;
    }

    private static Patient CreatePatient(
        int id = 0,
        string firstName = "John",
        string lastName = "Doe",
        bool isArchived = false)
    {
        return new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            IsArchived = isArchived
        };
    }

    private static MedicalHistory CreateHistory(
        int patientId,
        BloodType? bloodType = BloodType.A,
        Rh? rh = Rh.Positive)
    {
        return new MedicalHistory
        {
            PatientId = patientId,
            BloodType = bloodType,
            Rh = rh,
            ChronicConditions = new List<string>()
        };
    }

    private static MedicalRecord CreateRecord(
        int historyId,
        int staffId = 1,
        SourceType? sourceType = null,
        DateTime? consultationDate = null)
    {
        return new MedicalRecord
        {
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

    private static Prescription CreatePrescription(
        int recordId,
        DateTime? date = null,
        string medName = "Aspirin")
    {
        return new Prescription
        {
            RecordId = recordId,
            Date = date ?? DateTime.Today,
            MedicationList = new List<PrescriptionItem>
            {
                CreatePrescriptionItem(medName)
            }
        };
    }

    private static PrescriptionItem CreatePrescriptionItem(string medName)
    {
        return new PrescriptionItem
        {
            MedName = medName
        };
    }

    private static SourceType GetNonERSourceType()
    {
        return Enum.GetValues<SourceType>()
            .First(sourceType => sourceType != SourceType.ER);
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
}