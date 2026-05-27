using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Repository;

[TestClass]
public sealed class MedicalHistoryRepositoryTests
{
    private static EFHospitalDbContext CreateContext()
    {
        DbContextOptions<EFHospitalDbContext> options =
            new DbContextOptionsBuilder<EFHospitalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new EFHospitalDbContext(options);
    }

    private static MedicalHistoryRepository CreateRepository(EFHospitalDbContext context)
    {
        return new MedicalHistoryRepository(context);
    }

    [TestMethod]
    public async Task CreateAsync_WhenHistoryIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.CreateAsync(null!));

        Assert.AreEqual("history", result.ParamName);
    }

    [TestMethod]
    public async Task CreateAsync_WhenHistoryIsValid_ReturnsGeneratedId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        int result = await repository.CreateAsync(CreateHistory());

        Assert.AreNotEqual(0, result);
    }

    [TestMethod]
    public void Create_WhenHistoryIsValid_ReturnsGeneratedId()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        int result = repository.Create(CreateHistory());

        Assert.AreNotEqual(0, result);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenHistoryIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        ArgumentNullException result =
            await ThrowsAsync<ArgumentNullException>(() =>
                repository.UpdateAsync(null!));

        Assert.AreEqual("history", result.ParamName);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenHistoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(id: 999, patientId: 1);

        KeyNotFoundException result =
            await ThrowsAsync<KeyNotFoundException>(() =>
                repository.UpdateAsync(history));

        Assert.IsTrue(result.Message.Contains("Medical history 999 was not found."));
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientIdChanges_ThrowsInvalidOperationException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory existing = CreateHistory(patientId: 1);
        context.MedicalHistory.Add(existing);
        await context.SaveChangesAsync();

        int historyId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalHistory updated = CreateHistory(
            id: historyId,
            patientId: 2);

        InvalidOperationException result =
            await ThrowsAsync<InvalidOperationException>(() =>
                repository.UpdateAsync(updated));

        Assert.AreEqual("Cannot reassign medical history to another patient.", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenHistoryIsValid_UpdatesBloodType()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory existing = CreateHistory(
            patientId: 1,
            bloodType: BloodType.A);

        context.MedicalHistory.Add(existing);
        await context.SaveChangesAsync();

        int historyId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalHistory updated = CreateHistory(
            id: historyId,
            patientId: 1,
            bloodType: BloodType.B);

        await repository.UpdateAsync(updated);

        Assert.AreEqual(BloodType.B, context.MedicalHistory.Single().BloodType);
    }

    [TestMethod]
    public void Update_WhenHistoryIsValid_UpdatesBloodType()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory existing = CreateHistory(
            patientId: 1,
            bloodType: BloodType.A);

        context.MedicalHistory.Add(existing);
        context.SaveChanges();

        int historyId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalHistory updated = CreateHistory(
            id: historyId,
            patientId: 1,
            bloodType: BloodType.B);

        repository.Update(updated);

        Assert.AreEqual(BloodType.B, context.MedicalHistory.Single().BloodType);
    }

    [TestMethod]
    public async Task GetByPatientIdAsync_WhenHistoryDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory? result = await repository.GetByPatientIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenHistoryDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory? result = await repository.GetByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SaveAllergiesAsync_WhenAllergiesIsNull_DoesNotAddPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        await repository.SaveAllergiesAsync(1, null!);

        Assert.AreEqual(0, context.PatientAllergies.Count());
    }

    [TestMethod]
    public async Task SaveAllergiesAsync_WhenAllergiesIsEmpty_DoesNotAddPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        await repository.SaveAllergiesAsync(
            1,
            new List<(Allergy Allergy, string SeverityLevel)>());

        Assert.AreEqual(0, context.PatientAllergies.Count());
    }

    [TestMethod]
    public async Task SaveAllergiesAsync_WhenAllergiesExist_AddsPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        await context.SaveChangesAsync();

        await repository.SaveAllergiesAsync(
            history.Id,
            new List<(Allergy Allergy, string SeverityLevel)>
            {
                (allergy, "Mild")
            });

        Assert.AreEqual(1, context.PatientAllergies.Count());
    }

    [TestMethod]
    public void SaveAllergies_WhenAllergiesExist_AddsPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        context.SaveChanges();

        repository.SaveAllergies(
            history.Id,
            new List<(Allergy Allergy, string SeverityLevel)>
            {
                (allergy, "Mild")
            });

        Assert.AreEqual(1, context.PatientAllergies.Count());
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_WhenHistoryDoesNotExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        List<string> result = await repository.GetChronicConditionsAsync(999);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_WhenChronicConditionsExist_ReturnsConditions()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(
            chronicConditions: new List<string> { "Asthma", "Diabetes" });

        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        int historyId = history.Id;

        context.ChangeTracker.Clear();

        List<string> result = await repository.GetChronicConditionsAsync(historyId);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetChronicConditions_WhenChronicConditionsExist_ReturnsConditions()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(
            chronicConditions: new List<string> { "Asthma", "Diabetes" });

        context.MedicalHistory.Add(history);
        context.SaveChanges();

        int historyId = history.Id;

        context.ChangeTracker.Clear();

        List<string> result = repository.GetChronicConditions(historyId);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesByHistoryIdAsync_WhenNoAllergiesExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        List<(Allergy Allergy, string SeverityLevel)> result =
            await repository.GetAllergiesByHistoryIdAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAllergiesByHistoryIdAsync_WhenAllergiesExist_ReturnsAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        await context.SaveChangesAsync();

        context.PatientAllergies.Add(new PatientAllergy
        {
            MedicalHistoryId = history.Id,
            AllergyId = allergy.Id,
            Allergy = allergy,
            SeverityLevel = "Severe"
        });

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<(Allergy Allergy, string SeverityLevel)> result =
            await repository.GetAllergiesByHistoryIdAsync(history.Id);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetAllergiesByHistoryId_WhenAllergiesExist_ReturnsAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        context.SaveChanges();

        context.PatientAllergies.Add(new PatientAllergy
        {
            MedicalHistoryId = history.Id,
            AllergyId = allergy.Id,
            Allergy = allergy,
            SeverityLevel = "Severe"
        });

        context.SaveChanges();

        context.ChangeTracker.Clear();

        List<(Allergy Allergy, string SeverityLevel)> result =
            repository.GetAllergiesByHistoryId(history.Id);

        Assert.AreEqual(1, result.Count);
    }

    private static MedicalHistory CreateHistory(
        int id = 0,
        int patientId = 1,
        BloodType? bloodType = BloodType.A,
        Rh? rh = Rh.Positive,
        List<string>? chronicConditions = null)
    {
        return new MedicalHistory
        {
            Id = id,
            PatientId = patientId,
            BloodType = bloodType,
            Rh = rh,
            ChronicConditions = chronicConditions ?? new List<string>()
        };
    }

    private static Allergy CreateAllergy()
    {
        return new Allergy
        {
            AllergyName = "Peanut",
            AllergyType = "Food",
            AllergyCategory = "Food"
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

    [TestMethod]
    public void Create_WhenHistoryIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        ArgumentNullException result =
            Throws<ArgumentNullException>(() =>
                repository.Create(null!));

        Assert.AreEqual("history", result.ParamName);
    }

    [TestMethod]
    public void Update_WhenHistoryIsNull_ThrowsArgumentNullException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        ArgumentNullException result =
            Throws<ArgumentNullException>(() =>
                repository.Update(null!));

        Assert.AreEqual("history", result.ParamName);
    }

    [TestMethod]
    public void Update_WhenHistoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(id: 999, patientId: 1);

        KeyNotFoundException result =
            Throws<KeyNotFoundException>(() =>
                repository.Update(history));

        Assert.IsTrue(result.Message.Contains("Medical history 999 was not found."));
    }

    [TestMethod]
    public void Update_WhenPatientIdChanges_ThrowsInvalidOperationException()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory existing = CreateHistory(patientId: 1);
        context.MedicalHistory.Add(existing);
        context.SaveChanges();

        int historyId = existing.Id;

        context.ChangeTracker.Clear();

        MedicalHistory updated = CreateHistory(
            id: historyId,
            patientId: 2);

        InvalidOperationException result =
            Throws<InvalidOperationException>(() =>
                repository.Update(updated));

        Assert.AreEqual("Cannot reassign medical history to another patient.", result.Message);
    }

    [TestMethod]
    public void GetByPatientId_WhenHistoryDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory? result = repository.GetByPatientId(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_WhenHistoryDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory? result = repository.GetById(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void SaveAllergies_WhenAllergiesIsNull_DoesNotAddPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        repository.SaveAllergies(1, null!);

        Assert.AreEqual(0, context.PatientAllergies.Count());
    }

    [TestMethod]
    public void SaveAllergies_WhenAllergiesIsEmpty_DoesNotAddPatientAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        repository.SaveAllergies(
            1,
            new List<(Allergy Allergy, string SeverityLevel)>());

        Assert.AreEqual(0, context.PatientAllergies.Count());
    }

    [TestMethod]
    public async Task SaveAllergiesAsync_WhenAllergiesExist_SavesSeverityLevel()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        await context.SaveChangesAsync();

        await repository.SaveAllergiesAsync(
            history.Id,
            new List<(Allergy Allergy, string SeverityLevel)>
            {
            (allergy, "Severe")
            });

        Assert.AreEqual("Severe", context.PatientAllergies.Single().SeverityLevel);
    }

    [TestMethod]
    public async Task GetAllergiesByHistoryIdAsync_WhenAllergiesExist_ReturnsCorrectSeverity()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory();
        Allergy allergy = CreateAllergy();

        context.MedicalHistory.Add(history);
        context.Allergies.Add(allergy);
        await context.SaveChangesAsync();

        context.PatientAllergies.Add(new PatientAllergy
        {
            MedicalHistoryId = history.Id,
            AllergyId = allergy.Id,
            Allergy = allergy,
            SeverityLevel = "High"
        });

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        List<(Allergy Allergy, string SeverityLevel)> result =
            await repository.GetAllergiesByHistoryIdAsync(history.Id);

        Assert.AreEqual("High", result.Single().SeverityLevel);
    }

    [TestMethod]
    public void GetAllergiesByHistoryId_WhenNoAllergiesExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        List<(Allergy Allergy, string SeverityLevel)> result =
            repository.GetAllergiesByHistoryId(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetChronicConditions_WhenHistoryDoesNotExist_ReturnsEmptyList()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        List<string> result = repository.GetChronicConditions(999);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_WhenHistoryExists_ReturnsFirstCondition()
    {
        using EFHospitalDbContext context = CreateContext();
        MedicalHistoryRepository repository = CreateRepository(context);

        MedicalHistory history = CreateHistory(
            chronicConditions: new List<string> { "Asthma", "Diabetes" });

        context.MedicalHistory.Add(history);
        await context.SaveChangesAsync();

        int historyId = history.Id;

        context.ChangeTracker.Clear();

        List<string> result = await repository.GetChronicConditionsAsync(historyId);

        Assert.AreEqual("Asthma", result.First());
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
}