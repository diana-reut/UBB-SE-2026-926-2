using System.Reflection;
using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class PatientRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Patient MakePatient(
        int id = 1,
        string cnp = "1234567890123",
        string firstName = "Jane",
        string lastName = "Doe",
        bool isArchived = false,
        DateTime? dob = null,
        Sex sex = Sex.F) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Cnp = cnp,
            Dob = dob ?? new DateTime(1990, 1, 1),
            Sex = sex,
            PhoneNo = "0712345678",
            EmergencyContact = "John Doe",
            IsArchived = isArchived
        };

    private static MedicalHistory MakeHistory(int patientId, BloodType? bloodType = BloodType.A, Rh? rh = Rh.Positive, List<string>? conditions = null) =>
        new()
        {
            PatientId = patientId,
            BloodType = bloodType,
            Rh = rh,
            ChronicConditions = conditions ?? new List<string>()
        };

    private static int InvokeCalculateTotalScore(PatientRepository repository, Patient donor, BloodType bloodType, Rh rh, Sex sex, int age)
    {
        object[] arguments = new object[] { donor, bloodType, rh, sex, age };
        MethodInfo? method = typeof(PatientRepository).GetMethod("CalculateTotalScore", BindingFlags.Instance | BindingFlags.NonPublic);

        object? result = method!.Invoke(repository, arguments);

        return (int)result!;
    }

    private static bool InvokeIsRhMatch(Rh? donor, Rh receiver)
    {
        object[] arguments = new object[] { donor, receiver };
        MethodInfo? method = typeof(PatientRepository).GetMethod("IsARhMatch", BindingFlags.Static | BindingFlags.NonPublic);

        object? result = method!.Invoke(null, arguments);

        return (bool)result!;
    }

    [TestMethod]
    public async Task AddAsync_WhenPatientIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = new PatientRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(null!));
    }

    [TestMethod]
    public async Task GetAllAsync_WhenIncludeArchivedIsFalse_ExcludesArchivedPatients()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, isArchived: false),
            MakePatient(2, cnp: "2234567890123", isArchived: true));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetAllAsync(false);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task AddAsync_WhenPatientIsValid_PersistsPatient()
    {
        await using var context = CreateContext();
        var sut = new PatientRepository(context);

        await sut.AddAsync(MakePatient());

        Assert.AreEqual(1, context.Patients.Count());
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientExists_UpdatesPhoneNumber()
    {
        await using var context = CreateContext();
        Patient patient = MakePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);
        patient.PhoneNo = "0799999999";

        await sut.UpdateAsync(patient);

        Assert.AreEqual("0799999999", context.Patients.Single().PhoneNo);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPatientIsDetached_UpdatesPhoneNumber()
    {
        await using var context = CreateContext();
        Patient patient = MakePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        context.Entry(patient).State = EntityState.Detached;
        patient.PhoneNo = "0788888888";
        var sut = new PatientRepository(context);

        await sut.UpdateAsync(patient);

        Assert.AreEqual("0788888888", context.Patients.Single().PhoneNo);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPatientExists_RemovesPatient()
    {
        await using var context = CreateContext();
        Patient patient = MakePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        await sut.DeleteAsync(patient.Id);

        Assert.AreEqual(0, context.Patients.Count());
    }

    [TestMethod]
    public async Task ExistsAsync_WhenPatientExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.Patients.Add(MakePatient());
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        bool result = await sut.ExistsAsync("1234567890123");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetAllAsync_WhenIncludeArchivedIsTrue_ReturnsArchivedAndActivePatients()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, isArchived: false),
            MakePatient(2, cnp: "2234567890123", isArchived: true));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetAllAsync(true);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetArchivedAsync_WhenArchivedPatientsExist_ReturnsArchivedPatients()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, isArchived: false),
            MakePatient(2, cnp: "2234567890123", isArchived: true));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetArchivedAsync();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenMedicalHistoryExists_ReturnsPatientWithHistory()
    {
        await using var context = CreateContext();
        Patient patient = MakePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(patient.Id));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        Patient? result = await sut.GetByIdAsync(patient.Id);

        Assert.IsNotNull(result!.MedicalHistory);
    }

    [TestMethod]
    public async Task SearchAsync_WhenNamePartMatchesLastName_ReturnsMatchingPatient()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, lastName: "Doe"),
            MakePatient(2, cnp: "2234567890123", lastName: "Smith"));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { NamePart = "Do" });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenBloodTypeMatches_ReturnsMatchingPatient()
    {
        await using var context = CreateContext();
        Patient patient1 = MakePatient(1);
        Patient patient2 = MakePatient(2, cnp: "2234567890123");
        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();
        context.MedicalHistory.AddRange(
            MakeHistory(patient1.Id, bloodType: BloodType.A),
            MakeHistory(patient2.Id, bloodType: BloodType.B));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { BloodType = BloodType.B });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenHasChronicConditionIsTrue_ReturnsOnlyPatientsWithConditions()
    {
        await using var context = CreateContext();
        Patient patient1 = MakePatient(1);
        Patient patient2 = MakePatient(2, cnp: "2234567890123");
        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();
        context.MedicalHistory.AddRange(
            MakeHistory(patient1.Id, conditions: new List<string> { "Asthma" }),
            MakeHistory(patient2.Id, conditions: new List<string>()));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { HasChronicCond = true });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenCnpPrefixMatches_ReturnsMatchingPatient()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, cnp: "1234567890123"),
            MakePatient(2, cnp: "2234567890123"));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { CNP = "123" });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenFilterIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = new PatientRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SearchAsync(null!));
    }

    [TestMethod]
    public async Task SearchAsync_WhenMinAgeMatches_ReturnsOlderPatient()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, dob: new DateTime(DateTime.Now.Year - 40, 1, 1)),
            MakePatient(2, cnp: "2234567890123", dob: new DateTime(DateTime.Now.Year - 10, 1, 1)));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { MinAge = 18 });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenMaxAgeMatches_ReturnsYoungerPatient()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, dob: new DateTime(DateTime.Now.Year - 40, 1, 1)),
            MakePatient(2, cnp: "2234567890123", dob: new DateTime(DateTime.Now.Year - 10, 1, 1)));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { MaxAge = 18 });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WhenSexMatches_ReturnsMatchingPatient()
    {
        await using var context = CreateContext();
        context.Patients.AddRange(
            MakePatient(1, sex: Sex.F),
            MakePatient(2, cnp: "2234567890123", sex: Sex.M));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.SearchAsync(new PatientFilter { Sex = Sex.M });

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task MarkAsDeceasedAsync_WhenPatientExists_SetsArchivedTrue()
    {
        await using var context = CreateContext();
        Patient patient = MakePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        await sut.MarkAsDeceasedAsync(patient.Id, new DateTime(2026, 1, 1));

        Assert.IsTrue(context.Patients.Single().IsArchived);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenArchivedDonorExists_ExcludesArchivedDonor()
    {
        await using var context = CreateContext();
        Patient archivedDonor = MakePatient(1, isArchived: true);
        Patient activeDonor = MakePatient(2, cnp: "2234567890123");
        context.Patients.AddRange(archivedDonor, activeDonor);
        await context.SaveChangesAsync();
        context.MedicalHistory.AddRange(
            MakeHistory(archivedDonor.Id, bloodType: BloodType.O, rh: Rh.Negative),
            MakeHistory(activeDonor.Id, bloodType: BloodType.O, rh: Rh.Negative));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorHasChronicCondition_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient chronicDonor = MakePatient(1);
        Patient healthyDonor = MakePatient(2, cnp: "2234567890123");
        context.Patients.AddRange(chronicDonor, healthyDonor);
        await context.SaveChangesAsync();
        context.MedicalHistory.AddRange(
            MakeHistory(chronicDonor.Id, bloodType: BloodType.O, rh: Rh.Negative, conditions: new List<string> { "Asthma" }),
            MakeHistory(healthyDonor.Id, bloodType: BloodType.O, rh: Rh.Negative));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenAnaphylacticAllergyExists_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient patient1 = MakePatient(1);
        Patient patient2 = MakePatient(2, cnp: "2234567890123");
        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();
        MedicalHistory history1 = MakeHistory(patient1.Id, bloodType: BloodType.O, rh: Rh.Negative);
        MedicalHistory history2 = MakeHistory(patient2.Id, bloodType: BloodType.O, rh: Rh.Negative);
        context.MedicalHistory.AddRange(history1, history2);
        context.Allergies.Add(new Allergy { Id = 10, AllergyName = "Peanuts" });
        await context.SaveChangesAsync();
        context.PatientAllergies.Add(new PatientAllergy { MedicalHistoryId = history1.Id, AllergyId = 10, SeverityLevel = "Anaphylactic", Allergy = context.Allergies.Single(), MedicalHistory = history1 });
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenExactBloodAndRhMatchExists_RanksExactMatchFirst()
    {
        await using var context = CreateContext();
        Patient exactMatch = MakePatient(1, sex: Sex.F, dob: new DateTime(DateTime.Now.Year - 35, 1, 1));
        Patient universalMatch = MakePatient(2, cnp: "2234567890123", sex: Sex.M, dob: new DateTime(DateTime.Now.Year - 50, 1, 1));
        context.Patients.AddRange(exactMatch, universalMatch);
        await context.SaveChangesAsync();
        context.MedicalHistory.AddRange(
            MakeHistory(exactMatch.Id, bloodType: BloodType.A, rh: Rh.Positive),
            MakeHistory(universalMatch.Id, bloodType: BloodType.O, rh: Rh.Negative));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.A, Rh.Positive, Sex.F, new DateTime(DateTime.Now.Year - 34, 1, 1), 18, 60);

        Assert.AreEqual(1, result[0].Id);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenOnlyUniversalBloodMatchExists_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1, dob: new DateTime(DateTime.Now.Year - 45, 1, 1));
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.O, rh: Rh.Negative));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.M, new DateTime(DateTime.Now.Year - 20, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsAAndReceiverIsAB_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.A, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsBAndReceiverIsAB_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.B, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsBAndReceiverIsB_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.B, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.B, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsAAndReceiverIsB_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.A, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.B, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsABAndReceiverIsA_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.AB, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.A, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenPositiveDonorAndNegativeReceiver_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.O, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Negative, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenBloodMatchesAndRhDoesNot_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.A, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.A, Rh.Negative, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsABAndReceiverIsAB_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.AB, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorBloodTypeIsNull_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: null, rh: Rh.Positive));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenDonorRhIsNull_ExcludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.O, rh: null));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetCompatibleDonorsAsync_WhenNegativeDonorAndPositiveReceiver_IncludesDonor()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1);
        context.Patients.Add(donor);
        await context.SaveChangesAsync();
        context.MedicalHistory.Add(MakeHistory(donor.Id, bloodType: BloodType.O, rh: Rh.Negative));
        await context.SaveChangesAsync();
        var sut = new PatientRepository(context);

        List<Patient> result = await sut.GetCompatibleDonorsAsync(BloodType.AB, Rh.Positive, Sex.F, new DateTime(1990, 1, 1), 18, 60);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task CalculateTotalScore_WhenBloodMatchesAndRhDoesNot_UsesPartialCompatibilityScore()
    {
        await using var context = CreateContext();
        Patient donor = MakePatient(1, dob: new DateTime(DateTime.Now.Year - 30, 1, 1));
        donor.MedicalHistory = MakeHistory(donor.Id, bloodType: BloodType.A, rh: Rh.Negative);
        var sut = new PatientRepository(context);

        int score = InvokeCalculateTotalScore(sut, donor, BloodType.A, Rh.Positive, Sex.F, 30);

        Assert.AreEqual(75, score);
    }

    [TestMethod]
    public void IsARhMatch_WhenPositiveDonorAndPositiveReceiver_ReturnsTrue()
    {
        bool result = InvokeIsRhMatch(Rh.Positive, Rh.Positive);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsARhMatch_WhenPositiveDonorAndNegativeReceiver_ReturnsFalse()
    {
        bool result = InvokeIsRhMatch(Rh.Positive, Rh.Negative);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsARhMatch_WhenNegativeDonorAndPositiveReceiver_ReturnsTrue()
    {
        bool result = InvokeIsRhMatch(Rh.Negative, Rh.Positive);

        Assert.IsTrue(result);
    }
}
