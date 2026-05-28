using System.Reflection;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class PatientServiceTests
{
    private Mock<IPatientRepository> _patientRepository = null!;
    private Mock<IMedicalHistoryRepository> _historyRepository = null!;
    private Mock<IMedicalRecordRepository> _recordRepository = null!;
    private Mock<IPrescriptionRepository> _prescriptionRepository = null!;
    private PatientService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _patientRepository = new Mock<IPatientRepository>();
        _historyRepository = new Mock<IMedicalHistoryRepository>();
        _recordRepository = new Mock<IMedicalRecordRepository>();
        _prescriptionRepository = new Mock<IPrescriptionRepository>();
        _sut = new PatientService(
            _patientRepository.Object,
            _historyRepository.Object,
            _recordRepository.Object,
            _prescriptionRepository.Object);
    }

    private static Patient MakePatient(int id = 1, string cnp = "2900101123457") => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Doe",
        Cnp = cnp,
        PhoneNo = "0712345678",
        EmergencyContact = "John Doe",
        Dob = new DateTime(1990, 1, 1),
        Sex = Sex.F
    };

    private static DbUpdateException MakeDuplicateCnpUpdateException(
        int number = 2601,
        string message = "IX_Patient_CNP")
    {
        object? createdErrors = Activator.CreateInstance(
            typeof(SqlErrorCollection),
            nonPublic: true);
        SqlErrorCollection errors = (SqlErrorCollection)createdErrors!;
        ConstructorInfo sqlErrorConstructor = typeof(SqlError)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .OrderByDescending(constructor => constructor.GetParameters().Length)
            .First();
        object[] arguments = sqlErrorConstructor.GetParameters()
            .Select(parameter => parameter.ParameterType == typeof(int) ? (object)number
                : parameter.ParameterType == typeof(byte) ? (object)(byte)0
                : parameter.ParameterType == typeof(string) ? message
                : parameter.ParameterType == typeof(Exception) ? null
                : parameter.HasDefaultValue ? parameter.DefaultValue
                : parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType)
                : null!)
            .ToArray();
        var error = (SqlError)sqlErrorConstructor.Invoke(arguments);
        object[] addArguments = new object[] { error };
        MethodInfo? addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);
        addMethod!.Invoke(errors, addArguments);
        MethodInfo createException = typeof(SqlException)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .First(method => method.Name == "CreateException"
                && method.GetParameters().Length >= 2
                && method.GetParameters()[0].ParameterType == typeof(SqlErrorCollection));
        object[] createArguments = createException.GetParameters()
            .Select(parameter => parameter.ParameterType == typeof(SqlErrorCollection) ? errors
                : parameter.ParameterType == typeof(string) ? "11.0.0"
                : parameter.HasDefaultValue ? parameter.DefaultValue
                : parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType)
                : null!)
            .ToArray();
        object? createdException = createException.Invoke(null, createArguments);
        var sqlException = (SqlException)createdException!;

        return new DbUpdateException("Duplicate CNP.", sqlException);
    }

    private static bool InvokeIsDuplicateCnpException(DbUpdateException exception)
    {
        object[] arguments = new object[] { exception };
        MethodInfo? method = typeof(PatientService).GetMethod("IsDuplicateCnpException", BindingFlags.Static | BindingFlags.NonPublic);

        object? result = method!.Invoke(null, arguments);

        return (bool)result!;
    }

    [TestMethod]
    public void ValidateCNP_WhenLengthIsInvalid_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("123", Sex.F, new DateTime(1990, 1, 1));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_WhenSexDoesNotMatchFirstDigit_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("2900101123457", Sex.M, new DateTime(1990, 1, 1));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_WhenDobDoesNotMatchEmbeddedDate_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("2900101123457", Sex.F, new DateTime(1991, 1, 1));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_WhenCnpMatchesSexAndDob_ReturnsTrue()
    {
        bool result = _sut.ValidateCNP("2900101123457", Sex.F, new DateTime(1990, 1, 1));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenPatientIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreatePatientAsync(null!));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenDobIsInFuture_ThrowsArgumentException()
    {
        Patient patient = MakePatient();
        patient.Dob = DateTime.Today.AddDays(1);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreatePatientAsync(patient));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenPatientAlreadyExists_ThrowsArgumentException()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreatePatientAsync(patient));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenCnpDoesNotMatchPatientData_ThrowsArgumentException()
    {
        Patient patient = MakePatient(cnp: "1900101123457");
        _patientRepository.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreatePatientAsync(patient));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenPatientIsValid_DelegatesToRepository()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);
        _patientRepository.Setup(x => x.AddAsync(patient)).Returns(Task.CompletedTask);

        await _sut.CreatePatientAsync(patient);

        _patientRepository.Verify(x => x.AddAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenPatientIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdatePatientAsync(null!));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenPhoneNumberIsInvalid_ThrowsArgumentException()
    {
        Patient patient = MakePatient();
        patient.PhoneNo = "bad";

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenCnpDoesNotMatchPatientData_ThrowsArgumentException()
    {
        Patient patient = MakePatient(cnp: "1900101123457");

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenPatientIsValid_DelegatesToRepository()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.UpdateAsync(patient)).Returns(Task.CompletedTask);

        await _sut.UpdatePatientAsync(patient);

        _patientRepository.Verify(x => x.UpdateAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenPhoneNumberIsWhitespace_ThrowsArgumentException()
    {
        Patient patient = MakePatient();
        patient.PhoneNo = "   ";

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenRepositoryThrowsDuplicateCnp_ThrowsArgumentException()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.UpdateAsync(patient)).ThrowsAsync(MakeDuplicateCnpUpdateException());

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task ArchivePatientAsync_WhenCalled_SetsPatientArchived()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.UpdateAsync(patient)).Returns(Task.CompletedTask);

        await _sut.ArchivePatientAsync(patient);

        Assert.IsTrue(patient.IsArchived);
    }

    [TestMethod]
    public async Task DearchivePatientAsync_WhenPatientExists_SetsArchivedFalse()
    {
        Patient patient = MakePatient();
        patient.IsArchived = true;
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _patientRepository.Setup(x => x.UpdateAsync(patient)).Returns(Task.CompletedTask);

        await _sut.DearchivePatientAsync(1);

        Assert.IsFalse(patient.IsArchived);
    }

    [TestMethod]
    public async Task DearchivePatientAsync_WhenPatientDoesNotExist_ThrowsKeyNotFoundException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DearchivePatientAsync(1));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenDeathDateIsInFuture_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ArchiveAsDeceasedAsync(1, DateTime.Now.AddDays(1)));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenPatientDoesNotExist_ThrowsKeyNotFoundException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ArchiveAsDeceasedAsync(1, new DateTime(2026, 1, 1)));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenPatientExists_SetsDateOfDeath()
    {
        DateTime deathDate = new(2026, 1, 1);
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _patientRepository.Setup(x => x.UpdateAsync(patient)).Returns(Task.CompletedTask);

        await _sut.ArchiveAsDeceasedAsync(1, deathDate);

        Assert.AreEqual(deathDate, patient.Dod);
    }

    [TestMethod]
    public async Task DeletePatientAsync_WhenPatientDoesNotExist_ThrowsKeyNotFoundException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeletePatientAsync(1));
    }

    [TestMethod]
    public async Task DeletePatientAsync_WhenPatientExists_DelegatesToRepository()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _patientRepository.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

        await _sut.DeletePatientAsync(1);

        _patientRepository.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task ExistsAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
    {
        _patientRepository.Setup(x => x.ExistsAsync("2900101123457")).ReturnsAsync(true);

        bool result = await _sut.ExistsAsync("2900101123457");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenRepositoryReturnsPatient_ReturnsSameInstance()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

        Patient? result = await _sut.GetByIdAsync(1);

        Assert.AreSame(patient, result);
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenHistoryIsMissing_AssignsDefaultHistory()
    {
        Patient patient = MakePatient();
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        Patient result = await _sut.GetPatientDetailsAsync(1);

        Assert.IsNotNull(result.MedicalHistory);
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenPatientIsMissing_ThrowsKeyNotFoundException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPatientDetailsAsync(1));
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenHistoryExists_LoadsChronicConditions()
    {
        Patient patient = MakePatient();
        MedicalHistory history = new() { Id = 9, PatientId = 1 };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _historyRepository.Setup(x => x.GetChronicConditionsAsync(9)).ReturnsAsync(new List<string> { "Asthma" });
        _historyRepository.Setup(x => x.GetAllergiesByHistoryIdAsync(9)).ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>());
        _recordRepository.Setup(x => x.GetByHistoryIdAsync(9)).ReturnsAsync(new List<MedicalRecord>());

        Patient result = await _sut.GetPatientDetailsAsync(1);

        Assert.AreEqual("Asthma", result.MedicalHistory!.ChronicConditions[0]);
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenRecordsExist_OrdersRecordsDescendingByConsultationDate()
    {
        Patient patient = MakePatient();
        MedicalHistory history = new() { Id = 9, PatientId = 1 };
        MedicalRecord older = new() { Id = 1, HistoryId = 9, ConsultationDate = new DateTime(2026, 1, 1) };
        MedicalRecord newer = new() { Id = 2, HistoryId = 9, ConsultationDate = new DateTime(2026, 2, 1) };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _historyRepository.Setup(x => x.GetChronicConditionsAsync(9)).ReturnsAsync(new List<string>());
        _historyRepository.Setup(x => x.GetAllergiesByHistoryIdAsync(9)).ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>());
        _recordRepository.Setup(x => x.GetByHistoryIdAsync(9)).ReturnsAsync(new List<MedicalRecord> { older, newer });

        Patient result = await _sut.GetPatientDetailsAsync(1);

        Assert.AreEqual(2, result.MedicalHistory!.MedicalRecords[0].Id);
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenMinAgeIsNegative_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchPatientsAsync(new PatientFilter { MinAge = -1 }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenMaxAgeIsNegative_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchPatientsAsync(new PatientFilter { MaxAge = -1 }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenMinAgeExceedsMaxAge_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchPatientsAsync(new PatientFilter { MinAge = 10, MaxAge = 5 }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenCnpLengthIsInvalid_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchPatientsAsync(new PatientFilter { CNP = "123" }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenLastUpdatedFromIsAfterLastUpdatedTo_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchPatientsAsync(new PatientFilter
        {
            LastUpdatedFrom = new DateTime(2026, 2, 1),
            LastUpdatedTo = new DateTime(2026, 1, 1)
        }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenFilterIsValid_ReturnsRepositoryResults()
    {
        List<Patient> patients = new() { MakePatient() };
        PatientFilter filter = new() { MinAge = 18, MaxAge = 65, CNP = "2900101123457" };
        _patientRepository.Setup(x => x.SearchAsync(filter)).ReturnsAsync(patients);

        List<Patient> result = await _sut.SearchPatientsAsync(filter);

        Assert.AreSame(patients, result);
    }

    [TestMethod]
    public async Task IsHighRiskPatientAsync_WhenVisitCountExceedsThreshold_ReturnsTrue()
    {
        _recordRepository.Setup(x => x.GetERVisitCountAsync(1, It.IsAny<DateTime>())).ReturnsAsync(11);

        bool result = await _sut.IsHighRiskPatientAsync(1);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenPatientDoesNotExist_ThrowsArgumentException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMedicalHistoryAsync(1, new MedicalHistory()));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenAllergiesExist_SavesAllergies()
    {
        Allergy allergy = new() { Id = 4, AllergyName = "Peanuts" };
        MedicalHistory history = new()
        {
            Allergies = new List<(Allergy Allergy, string SeverityLevel)> { (allergy, "Severe") }
        };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.CreateAsync(It.IsAny<MedicalHistory>())).ReturnsAsync(9);

        await _sut.CreateMedicalHistoryAsync(1, history);

        _historyRepository.Verify(x => x.SaveAllergiesAsync(9, It.IsAny<List<(Allergy Allergy, string SeverityLevel)>>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenHistoryIdIsInvalid_DoesNotSaveAllergies()
    {
        MedicalHistory history = new()
        {
            Allergies = new List<(Allergy Allergy, string SeverityLevel)> { (new Allergy { Id = 4, AllergyName = "Peanuts" }, "Severe") }
        };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.CreateAsync(It.IsAny<MedicalHistory>())).ReturnsAsync(0);

        await _sut.CreateMedicalHistoryAsync(1, history);

        _historyRepository.Verify(x => x.SaveAllergiesAsync(It.IsAny<int>(), It.IsAny<List<(Allergy Allergy, string SeverityLevel)>>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenAllergiesAreNull_DoesNotSaveAllergies()
    {
        MedicalHistory history = new()
        {
            Allergies = null!
        };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.CreateAsync(It.IsAny<MedicalHistory>())).ReturnsAsync(9);

        await _sut.CreateMedicalHistoryAsync(1, history);

        _historyRepository.Verify(x => x.SaveAllergiesAsync(It.IsAny<int>(), It.IsAny<List<(Allergy Allergy, string SeverityLevel)>>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenAllergiesAreEmpty_DoesNotSaveAllergies()
    {
        MedicalHistory history = new()
        {
            Allergies = new List<(Allergy Allergy, string SeverityLevel)>()
        };
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepository.Setup(x => x.CreateAsync(It.IsAny<MedicalHistory>())).ReturnsAsync(9);

        await _sut.CreateMedicalHistoryAsync(1, history);

        _historyRepository.Verify(x => x.SaveAllergiesAsync(It.IsAny<int>(), It.IsAny<List<(Allergy Allergy, string SeverityLevel)>>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenPatientAlreadyHasHistory_ThrowsArgumentException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1 });

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMedicalHistoryAsync(1, new MedicalHistory()));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenHistoryIsNull_ThrowsArgumentException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMedicalHistoryAsync(1, null!));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenMedicalHistoryIsMissing_ThrowsInvalidOperationException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateMedicalRecordAsync(1, new MedicalRecord()));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenPatientDoesNotExist_ThrowsKeyNotFoundException()
    {
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateMedicalRecordAsync(1, new MedicalRecord()));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenMedicalHistoryExists_SetsHistoryId()
    {
        MedicalRecord record = new();
        _patientRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(MakePatient());
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1 });
        _recordRepository.Setup(x => x.AddAsync(record)).ReturnsAsync(12);

        await _sut.CreateMedicalRecordAsync(1, record);

        Assert.AreEqual(9, record.HistoryId);
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenRepositoryIsUnavailable_ThrowsInvalidOperationException()
    {
        var service = new PatientService(_patientRepository.Object, _historyRepository.Object, _recordRepository.Object, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePrescriptionAsync(1, new Prescription()));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenPrescriptionIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreatePrescriptionAsync(1, null!));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenRecordDoesNotExist_ThrowsKeyNotFoundException()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((MedicalRecord?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreatePrescriptionAsync(1, new Prescription()));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenRecordExists_SetsPrescriptionRecordId()
    {
        Prescription prescription = new();
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1 });
        _prescriptionRepository.Setup(x => x.AddAsync(prescription)).Returns(Task.CompletedTask);

        await _sut.CreatePrescriptionAsync(1, prescription);

        Assert.AreEqual(1, prescription.RecordId);
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenPatientIdIsInvalid_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetMedicalHistoryAsync(0));
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenRepositoryThrows_ReturnsNull()
    {
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ThrowsAsync(new Exception("boom"));

        MedicalHistory? result = await _sut.GetMedicalHistoryAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenHistoryExists_ReturnsHistory()
    {
        MedicalHistory history = new() { Id = 9, PatientId = 1 };
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(history);

        MedicalHistory? result = await _sut.GetMedicalHistoryAsync(1);

        Assert.AreSame(history, result);
    }

    [TestMethod]
    public async Task GetMedicalRecordsAsync_WhenRepositoryThrows_ReturnsEmptyList()
    {
        _recordRepository.Setup(x => x.GetByHistoryIdAsync(5)).ThrowsAsync(new Exception("boom"));

        List<MedicalRecord> result = await _sut.GetMedicalRecordsAsync(5);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetMedicalRecordsAsync_WhenRepositoryReturnsRecords_ReturnsRecords()
    {
        List<MedicalRecord> records = new() { new() { Id = 1, HistoryId = 5 } };
        _recordRepository.Setup(x => x.GetByHistoryIdAsync(5)).ReturnsAsync(records);

        List<MedicalRecord> result = await _sut.GetMedicalRecordsAsync(5);

        Assert.AreSame(records, result);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenRecordIsMissing_ThrowsKeyNotFoundException()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((MedicalRecord?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetRecordExportDataAsync(1));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPatientIsMissing_ThrowsKeyNotFoundException()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 9 });
        _historyRepository.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1 });

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetRecordExportDataAsync(1));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenHistoryIsMissing_ThrowsKeyNotFoundException()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 9 });
        _historyRepository.Setup(x => x.GetByIdAsync(9)).ReturnsAsync((MedicalHistory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetRecordExportDataAsync(1));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPrescriptionExists_ReturnsItems()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 9 });
        _historyRepository.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1, Patient = MakePatient() });
        _prescriptionRepository.Setup(x => x.GetByRecordIdAsync(1)).ReturnsAsync(new Prescription { Id = 4, RecordId = 1 });
        _prescriptionRepository.Setup(x => x.GetItemsAsync(4)).ReturnsAsync(new List<PrescriptionItem> { new PrescriptionItem { MedName = "Ibuprofen" } });

        RecordExportDataDto result = await _sut.GetRecordExportDataAsync(1);

        Assert.AreEqual(1, result.Items.Count);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPrescriptionRepositoryIsUnavailable_ReturnsNullPrescription()
    {
        var service = new PatientService(_patientRepository.Object, _historyRepository.Object, _recordRepository.Object, null);
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 9 });
        _historyRepository.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1, Patient = MakePatient() });

        RecordExportDataDto result = await service.GetRecordExportDataAsync(1);

        Assert.IsNull(result.Prescription);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPrescriptionIsMissing_ReturnsEmptyItems()
    {
        _recordRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 9 });
        _historyRepository.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1, Patient = MakePatient() });
        _prescriptionRepository.Setup(x => x.GetByRecordIdAsync(1)).ReturnsAsync((Prescription?)null);

        RecordExportDataDto result = await _sut.GetRecordExportDataAsync(1);

        Assert.AreEqual(0, result.Items.Count);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenAllergiesExist_ReturnsFormattedAllergy()
    {
        Allergy allergy = new() { Id = 1, AllergyName = "Peanuts" };
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 9, PatientId = 1 });
        _historyRepository.Setup(x => x.GetAllergiesByHistoryIdAsync(9)).ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)> { (allergy, "Severe") });

        List<string> result = await _sut.GetPatientAllergiesAsync(1);

        Assert.AreEqual("Peanuts - Severe", result[0]);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenHistoryDoesNotExist_ReturnsEmptyList()
    {
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        List<string> result = await _sut.GetPatientAllergiesAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenRepositoryThrows_ReturnsEmptyList()
    {
        _historyRepository.Setup(x => x.GetByPatientIdAsync(1)).ThrowsAsync(new Exception("boom"));

        List<string> result = await _sut.GetPatientAllergiesAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_WhenRepositoryIsUnavailable_ThrowsInvalidOperationException()
    {
        var service = new PatientService(_patientRepository.Object, _historyRepository.Object, _recordRepository.Object, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetPrescriptionByRecordIdAsync(5));
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_WhenRepositoryIsAvailable_DelegatesToRepository()
    {
        _prescriptionRepository.Setup(x => x.GetByRecordIdAsync(5)).ReturnsAsync(new Prescription { RecordId = 5 });

        await _sut.GetPrescriptionByRecordIdAsync(5);

        _prescriptionRepository.Verify(x => x.GetByRecordIdAsync(5), Times.Once);
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_WhenRepositoryReturnsPrescription_ReturnsPrescription()
    {
        Prescription prescription = new() { RecordId = 5 };
        _prescriptionRepository.Setup(x => x.GetByRecordIdAsync(5)).ReturnsAsync(prescription);

        Prescription? result = await _sut.GetPrescriptionByRecordIdAsync(5);

        Assert.AreSame(prescription, result);
    }

    [TestMethod]
    public void IsDuplicateCnpException_WhenInnerExceptionIsNotSqlException_ReturnsFalse()
    {
        bool result = InvokeIsDuplicateCnpException(new DbUpdateException("Boom.", new InvalidOperationException()));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsDuplicateCnpException_WhenSqlNumberIs2627_ReturnsTrue()
    {
        bool result = InvokeIsDuplicateCnpException(MakeDuplicateCnpUpdateException(2627));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsDuplicateCnpException_WhenSqlMessageDoesNotContainIndex_ReturnsFalse()
    {
        bool result = InvokeIsDuplicateCnpException(MakeDuplicateCnpUpdateException(message: "Other index"));

        Assert.IsFalse(result);
    }
}
