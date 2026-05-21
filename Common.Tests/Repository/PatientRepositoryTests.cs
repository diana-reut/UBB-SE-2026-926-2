using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Common.Tests.Repository;

[TestClass]
public sealed class PatientRepositoryTests
{
    private static EFHospitalDbContext CreateContext()
    {
        DbContextOptions<EFHospitalDbContext> options = new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EFHospitalDbContext(options);
    }

    [TestMethod]
    public async Task AddAsync_WhenPatientIsNull_Throws()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [TestMethod]
    public async Task AddAsync_AddsPatientAndPersistsChanges()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);
        Patient patient = CreatePatient(1, "Ana", "Pop", "1960101012345");

        await repository.AddAsync(patient);

        Assert.AreEqual(1, await context.Patients.CountAsync());
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientIsNull_Throws()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => repository.UpdateAsync(null!));
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientIsTracked_UpdatesCurrentValues()
    {
        using EFHospitalDbContext context = CreateContext();
        Patient patient = CreatePatient(1, "Ana", "Pop", "1960101012345");
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        await repository.UpdateAsync(new Patient
        {
            Id = 1,
            FirstName = "Updated",
            LastName = "Pop",
            Cnp = "1960101012345",
            Dob = patient.Dob,
            Sex = Sex.M,
            PhoneNo = "0711111111",
            EmergencyContact = "Contact",
            IsArchived = true,
        });

        Patient stored = await context.Patients.SingleAsync();
        Assert.IsTrue(stored.FirstName == "Updated" && stored.IsArchived);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientIsNotTracked_UpdatesEntity()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.Add(CreatePatient(1, "Ana", "Pop", "1960101012345"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new PatientRepository(context);

        await repository.UpdateAsync(CreatePatient(1, "Mihai", "Pop", "1960101012345"));

        Patient stored = await context.Patients.SingleAsync();
        Assert.AreEqual("Mihai", stored.FirstName);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPatientExists_RemovesPatient()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.Add(CreatePatient(1, "Ana", "Pop", "1960101012345"));
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        await repository.DeleteAsync(1);

        Assert.AreEqual(0, await context.Patients.CountAsync());
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPatientDoesNotExist_DoesNothing()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        await repository.DeleteAsync(99);

        Assert.AreEqual(0, await context.Patients.CountAsync());
    }

    [TestMethod]
    public async Task ExistsAsync_ReturnsWhetherCnpExists()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.Add(CreatePatient(1, "Ana", "Pop", "1960101012345"));
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        Assert.IsTrue(await repository.ExistsAsync("1960101012345") && !await repository.ExistsAsync("2990101012345"));
    }

    [TestMethod]
    public async Task GetAllAsync_WhenArchivedExcluded_ReturnsOnlyActivePatientsWithHistory()
    {
        using EFHospitalDbContext context = CreateContext();
        Patient active = CreatePatient(1, "Ana", "Pop", "1960101012345");
        Patient archived = CreatePatient(2, "Ioana", "Ionescu", "2960101012345", isArchived: true);
        context.Patients.AddRange(active, archived);
        context.MedicalHistory.Add(new MedicalHistory { Id = 10, PatientId = 1, BloodType = BloodType.A });
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.GetAllAsync(false);

        Assert.IsTrue(result.Count == 1 && result[0].Id == 1 && result[0].MedicalHistory is not null);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenArchivedIncluded_ReturnsActiveAndArchivedPatients()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.AddRange(
            CreatePatient(1, "Ana", "Pop", "1960101012345"),
            CreatePatient(2, "Ioana", "Ionescu", "2960101012345", isArchived: true));
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.GetAllAsync(true);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetArchivedAsync_ReturnsOnlyArchivedPatients()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.AddRange(
            CreatePatient(1, "Ana", "Pop", "1960101012345"),
            CreatePatient(2, "Ioana", "Ionescu", "2960101012345", isArchived: true));
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.GetArchivedAsync();

        Assert.IsTrue(result.Count == 1 && result[0].Id == 2);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsPatientWithHistoryAndAllergies()
    {
        using EFHospitalDbContext context = CreateContext();
        Patient patient = CreatePatient(1, "Ana", "Pop", "1960101012345");
        var allergy = new Allergy { Id = 7, AllergyName = "Peanut" };
        var history = new MedicalHistory { Id = 10, PatientId = 1, BloodType = BloodType.A };
        var patientAllergy = new PatientAllergy
        {
            AllergyId = 7,
            MedicalHistoryId = 10,
            Allergy = allergy,
            MedicalHistory = history,
            SeverityLevel = "severe",
        };
        history.PatientAllergies.Add(patientAllergy);
        context.Patients.Add(patient);
        context.Allergies.Add(allergy);
        context.MedicalHistory.Add(history);
        context.PatientAllergies.Add(patientAllergy);
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        Patient? result = await repository.GetByIdAsync(1);

        Assert.AreEqual("Peanut", result?.MedicalHistory?.PatientAllergies[0].Allergy.AllergyName);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        Patient? result = await repository.GetByIdAsync(99);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SearchAsync_WhenFilterIsNull_Throws()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => repository.SearchAsync(null!));
    }

    [TestMethod]
    public async Task SearchAsync_AppliesAllSupportedFilters()
    {
        using EFHospitalDbContext context = CreateContext();
        int currentYear = DateTime.Now.Year;
        context.Patients.AddRange(
            CreatePatient(1, "Ana", "Popescu", "1960101012345", new DateTime(currentYear - 30, 1, 1), Sex.M),
            CreatePatient(2, "Maria", "Ionescu", "2960101012345", new DateTime(currentYear - 50, 1, 1), Sex.F),
            CreatePatient(3, "Ion", "Popa", "1960202012345", new DateTime(currentYear - 20, 1, 1), Sex.M));
        context.MedicalHistory.AddRange(
            new MedicalHistory { Id = 10, PatientId = 1, BloodType = BloodType.A, ChronicConditions = ["Asthma"] },
            new MedicalHistory { Id = 20, PatientId = 2, BloodType = BloodType.B, ChronicConditions = ["Diabetes"] },
            new MedicalHistory { Id = 30, PatientId = 3, BloodType = BloodType.A, ChronicConditions = [] });
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.SearchAsync(new PatientFilter
        {
            NamePart = "Pop",
            CNP = "196",
            MinAge = 25,
            MaxAge = 35,
            BloodType = BloodType.A,
            Sex = Sex.M,
            HasChronicCond = true,
        });

        Assert.IsTrue(result.Count == 1 && result[0].Id == 1);
    }

    [TestMethod]
    public async Task SearchAsync_WhenOptionalFiltersAreEmpty_ReturnsAllPatients()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.AddRange(
            CreatePatient(1, "Ana", "Pop", "1960101012345"),
            CreatePatient(2, "Maria", "Ionescu", "2960101012345"));
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.SearchAsync(new PatientFilter
        {
            NamePart = " ",
            CNP = " ",
            HasChronicCond = false,
        });

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task MarkAsDeceasedAsync_WhenPatientExists_SetsDodAndArchives()
    {
        using EFHospitalDbContext context = CreateContext();
        context.Patients.Add(CreatePatient(1, "Ana", "Pop", "1960101012345"));
        await context.SaveChangesAsync();
        DateTime dod = new(2024, 1, 1);
        var repository = new PatientRepository(context);

        await repository.MarkAsDeceasedAsync(1, dod);

        Patient stored = await context.Patients.SingleAsync();
        Assert.IsTrue(stored.Dod == dod && stored.IsArchived);
    }

    [TestMethod]
    public async Task MarkAsDeceasedAsync_WhenPatientDoesNotExist_DoesNothing()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);

        await repository.MarkAsDeceasedAsync(99, DateTime.Today);

        Assert.AreEqual(0, await context.Patients.CountAsync());
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_FiltersAndOrdersDonorsByCompatibilityScore()
    {
        using EFHospitalDbContext context = CreateContext();
        int currentYear = DateTime.Now.Year;
        context.Patients.AddRange(
            CreatePatient(1, "Exact", "Match", "1960101012345", new DateTime(currentYear - 40, 1, 1), Sex.M),
            CreatePatient(2, "Universal", "Donor", "1960202012345", new DateTime(currentYear - 50, 1, 1), Sex.F),
            CreatePatient(3, "Archived", "Donor", "1960303012345", new DateTime(currentYear - 40, 1, 1), Sex.M, true),
            CreatePatient(4, "Chronic", "Donor", "1960404012345", new DateTime(currentYear - 40, 1, 1), Sex.M),
            CreatePatient(5, "Allergy", "Donor", "1960505012345", new DateTime(currentYear - 40, 1, 1), Sex.M),
            CreatePatient(6, "Wrong", "Blood", "1960606012345", new DateTime(currentYear - 40, 1, 1), Sex.M));
        var allergy = new Allergy { Id = 1, AllergyName = "Peanut" };
        var allergyHistory = new MedicalHistory { Id = 50, PatientId = 5, BloodType = BloodType.A, Rh = Rh.Positive };
        var patientAllergy = new PatientAllergy
        {
            AllergyId = 1,
            MedicalHistoryId = 50,
            Allergy = allergy,
            MedicalHistory = allergyHistory,
            SeverityLevel = "Anaphylactic",
        };
        allergyHistory.PatientAllergies.Add(patientAllergy);
        context.MedicalHistory.AddRange(
            new MedicalHistory { Id = 10, PatientId = 1, BloodType = BloodType.A, Rh = Rh.Positive },
            new MedicalHistory { Id = 20, PatientId = 2, BloodType = BloodType.O, Rh = Rh.Negative },
            new MedicalHistory { Id = 30, PatientId = 3, BloodType = BloodType.A, Rh = Rh.Positive },
            new MedicalHistory { Id = 40, PatientId = 4, BloodType = BloodType.A, Rh = Rh.Positive, ChronicConditions = ["Asthma"] },
            allergyHistory,
            new MedicalHistory { Id = 60, PatientId = 6, BloodType = BloodType.B, Rh = Rh.Positive });
        context.Allergies.Add(allergy);
        context.PatientAllergies.Add(patientAllergy);
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.GetCompatibleDonorsAsync(
            BloodType.A,
            Rh.Positive,
            Sex.M,
            new DateTime(currentYear - 40, 1, 1),
            18,
            65);

        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Select(p => p.Id).ToArray());
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_CoversAllBloodTypeAndRhCompatibilityBranches()
    {
        using EFHospitalDbContext context = CreateContext();
        int currentYear = DateTime.Now.Year;
        DateTime donorDob = new(currentYear - 40, 1, 1);
        context.Patients.AddRange(
            CreatePatient(1, "Null", "Blood", "1960101012345", donorDob, Sex.F),
            CreatePatient(2, "Null", "Rh", "1960202012345", donorDob, Sex.F),
            CreatePatient(3, "A", "ToAB", "1960303012345", donorDob, Sex.F),
            CreatePatient(4, "B", "ToAB", "1960404012345", donorDob, Sex.F),
            CreatePatient(5, "AB", "ToAB", "1960505012345", donorDob, Sex.F),
            CreatePatient(6, "APos", "ToNegative", "1960606012345", donorDob, Sex.F),
            CreatePatient(7, "Large", "AgeGap", "1960707012345", new DateTime(currentYear - 95, 1, 1), Sex.M));
        context.MedicalHistory.AddRange(
            new MedicalHistory { Id = 10, PatientId = 1, BloodType = null, Rh = Rh.Negative },
            new MedicalHistory { Id = 20, PatientId = 2, BloodType = BloodType.O, Rh = null },
            new MedicalHistory { Id = 30, PatientId = 3, BloodType = BloodType.A, Rh = Rh.Negative },
            new MedicalHistory { Id = 40, PatientId = 4, BloodType = BloodType.B, Rh = Rh.Negative },
            new MedicalHistory { Id = 50, PatientId = 5, BloodType = BloodType.AB, Rh = Rh.Negative },
            new MedicalHistory { Id = 60, PatientId = 6, BloodType = BloodType.A, Rh = Rh.Positive },
            new MedicalHistory { Id = 70, PatientId = 7, BloodType = BloodType.O, Rh = Rh.Negative });
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> abNegativeMatches = await repository.GetCompatibleDonorsAsync(
            BloodType.AB,
            Rh.Negative,
            Sex.F,
            donorDob,
            18,
            100);

        List<Patient> oNegativeMatches = await repository.GetCompatibleDonorsAsync(
            BloodType.O,
            Rh.Negative,
            Sex.F,
            donorDob,
            18,
            100);

        Assert.IsTrue(
            abNegativeMatches.Select(p => p.Id).OrderBy(id => id).SequenceEqual(new[] { 3, 4, 5, 7 })
            && oNegativeMatches.Select(p => p.Id).SequenceEqual(new[] { 7 }));
    }

    [DataTestMethod]
    [DataRow(BloodType.A, BloodType.B)]
    [DataRow(BloodType.A, BloodType.O)]
    [DataRow(BloodType.B, BloodType.A)]
    [DataRow(BloodType.B, BloodType.O)]
    [DataRow(BloodType.AB, BloodType.A)]
    [DataRow(BloodType.AB, BloodType.B)]
    [DataRow(BloodType.AB, BloodType.O)]
    public async Task GetCompatibleDonorsAsync_WhenBloodTypesAreIncompatible_ReturnsNoDonors(
        BloodType donorBloodType,
        BloodType recipientBloodType)
    {
        using EFHospitalDbContext context = CreateContext();
        int currentYear = DateTime.Now.Year;
        DateTime dob = new(currentYear - 40, 1, 1);
        context.Patients.Add(CreatePatient(1, "Donor", "Patient", "1960101012345", dob, Sex.M));
        context.MedicalHistory.Add(new MedicalHistory
        {
            Id = 10,
            PatientId = 1,
            BloodType = donorBloodType,
            Rh = Rh.Negative,
        });
        await context.SaveChangesAsync();
        var repository = new PatientRepository(context);

        List<Patient> result = await repository.GetCompatibleDonorsAsync(
            recipientBloodType,
            Rh.Negative,
            Sex.M,
            dob,
            18,
            65);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void PrivateCompatibilityHelpers_CoverRemainingBloodRhAndScoreBranches()
    {
        using EFHospitalDbContext context = CreateContext();
        var repository = new PatientRepository(context);
        MethodInfo scoreMethod = typeof(PatientRepository).GetMethod("CalculateTotalScore", BindingFlags.NonPublic | BindingFlags.Instance)!;
        MethodInfo bloodMatchMethod = typeof(PatientRepository).GetMethod("IsABloodMatch", BindingFlags.NonPublic | BindingFlags.Static)!;
        MethodInfo rhMatchMethod = typeof(PatientRepository).GetMethod("IsARhMatch", BindingFlags.NonPublic | BindingFlags.Static)!;

        int nullHistoryScore = (int)scoreMethod.Invoke(
            repository,
            [
                CreatePatient(1, "No", "History", "1960101012345"),
                BloodType.A,
                Rh.Positive,
                Sex.M,
                DateTime.Now.Year - 1996
            ])!;

        Assert.IsTrue(
            nullHistoryScore == 75
            && (bool)bloodMatchMethod.Invoke(null, [BloodType.B, BloodType.B])!
            && (bool)bloodMatchMethod.Invoke(null, [BloodType.B, BloodType.AB])!
            && !(bool)bloodMatchMethod.Invoke(null, [BloodType.AB, BloodType.B])!
            && (bool)rhMatchMethod.Invoke(null, [Rh.Positive, Rh.Positive])!
            && !(bool)rhMatchMethod.Invoke(null, [Rh.Positive, Rh.Negative])!
            && !(bool)rhMatchMethod.Invoke(null, [(Rh)999, Rh.Positive])!);
    }

    private static Patient CreatePatient(
        int id,
        string firstName,
        string lastName,
        string cnp,
        DateTime? dob = null,
        Sex sex = Sex.M,
        bool isArchived = false)
    {
        return new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Cnp = cnp,
            Dob = dob ?? new DateTime(1996, 1, 1),
            Sex = sex,
            PhoneNo = "0711111111",
            EmergencyContact = "Contact",
            IsArchived = isArchived,
        };
    }
}
