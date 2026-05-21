using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace Common.Tests.Service;

[TestClass]
public sealed class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IMedicalHistoryRepository> _historyRepo = new();
    private readonly Mock<IMedicalRecordRepository> _recordRepo = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepo = new();

    private PatientService CreateService(bool includePrescriptionRepository = true)
    {
        return new PatientService(
            _patientRepo.Object,
            _historyRepo.Object,
            _recordRepo.Object,
            includePrescriptionRepository ? _prescriptionRepo.Object : null);
    }

    [TestMethod]
    public void ValidateCNP_ReturnsFalseForInvalidFormats()
    {
        PatientService service = CreateService();

        Assert.IsTrue(
            !service.ValidateCNP("", Sex.M, new DateTime(1996, 1, 1))
            && !service.ValidateCNP("196010101234", Sex.M, new DateTime(1996, 1, 1))
            && !service.ValidateCNP("196010101234X", Sex.M, new DateTime(1996, 1, 1)));
    }

    [TestMethod]
    public void ValidateCNP_ReturnsFalseWhenSexOrDobDoesNotMatch()
    {
        PatientService service = CreateService();

        Assert.IsTrue(
            !service.ValidateCNP("1960101012345", Sex.F, new DateTime(1996, 1, 1))
            && !service.ValidateCNP("1960101012345", Sex.M, new DateTime(1996, 1, 2)));
    }

    [TestMethod]
    public void ValidateCNP_ReturnsTrueForMatchingMaleAndFemaleCnp()
    {
        PatientService service = CreateService();

        Assert.IsTrue(
            service.ValidateCNP("1960101012345", Sex.M, new DateTime(1996, 1, 1))
            && service.ValidateCNP("2960101012345", Sex.F, new DateTime(1996, 1, 1)));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenDataIsNull_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => service.CreatePatientAsync(null!));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenDobIsTodayOrFuture_Throws()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(dob: DateTime.Today);

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.CreatePatientAsync(patient), "Birth Date must be in the past");
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenCnpDoesNotMatch_Throws()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(cnp: "2960101012345", sex: Sex.M);

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.CreatePatientAsync(patient), "Identity Mismatch");
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenCnpAlreadyExists_Throws()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(true);

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.CreatePatientAsync(patient), "already exists");
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenRepositoryThrowsDuplicateCnpDbUpdateException_RethrowsDbUpdateException()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);
        _patientRepo.Setup(x => x.AddAsync(patient)).ThrowsAsync(new DbUpdateException("duplicate"));

        await Common.Tests.TestAssert.ThrowsExceptionAsync<DbUpdateException>(() => service.CreatePatientAsync(patient));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenRepositoryThrowsDuplicateCnpSqlException_ThrowsArgumentException()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);
        _patientRepo.Setup(x => x.AddAsync(patient))
            .ThrowsAsync(new DbUpdateException("duplicate", CreateSqlException(2601, "IX_Patient_CNP")));

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.CreatePatientAsync(patient), "already exists");
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenRepositoryThrowsWrongIndexSqlException_RethrowsDbUpdateException()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);
        _patientRepo.Setup(x => x.AddAsync(patient))
            .ThrowsAsync(new DbUpdateException("duplicate", CreateSqlException(2627, "Other_Index")));

        await Common.Tests.TestAssert.ThrowsExceptionAsync<DbUpdateException>(() => service.CreatePatientAsync(patient));
    }

    [TestMethod]
    public async Task CreatePatientAsync_WhenDataIsValid_AddsAndReturnsPatient()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.ExistsAsync(patient.Cnp)).ReturnsAsync(false);

        Patient result = await service.CreatePatientAsync(patient);

        Assert.AreSame(patient, result);
        _patientRepo.Verify(x => x.AddAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenDataIsNull_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => service.UpdatePatientAsync(null!));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenCnpDoesNotMatch_Throws()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(cnp: "2960101012345", sex: Sex.M);

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.UpdatePatientAsync(patient), "Identity Mismatch");
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenPhoneIsInvalid_Throws()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(phoneNo: "abc");

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.UpdatePatientAsync(patient), "Phone number");
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenRepositoryThrowsDbUpdateException_RethrowsDbUpdateException()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.UpdateAsync(patient)).ThrowsAsync(new DbUpdateException("duplicate"));

        await Common.Tests.TestAssert.ThrowsExceptionAsync<DbUpdateException>(() => service.UpdatePatientAsync(patient));
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenRepositoryThrowsDuplicateCnpSqlException_ThrowsArgumentException()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.UpdateAsync(patient))
            .ThrowsAsync(new DbUpdateException("duplicate", CreateSqlException(2627, "IX_Patient_CNP")));

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(() => service.UpdatePatientAsync(patient), "already exists");
    }

    [TestMethod]
    public async Task UpdatePatientAsync_WhenDataIsValid_UpdatesPatient()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(phoneNo: "+40 711111111");

        await service.UpdatePatientAsync(patient);

        _patientRepo.Verify(x => x.UpdateAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task ArchivePatientAsync_SetsArchivedAndUpdatesPatient()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();

        await service.ArchivePatientAsync(patient);

        Assert.IsTrue(patient.IsArchived);
        _patientRepo.Verify(x => x.UpdateAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task DearchivePatientAsync_WhenPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.DearchivePatientAsync(1));
    }

    [TestMethod]
    public async Task DearchivePatientAsync_WhenPatientExists_UnarchivesAndUpdates()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient(isArchived: true);
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

        await service.DearchivePatientAsync(1);

        Assert.IsFalse(patient.IsArchived);
        _patientRepo.Verify(x => x.UpdateAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenDeathDateIsFuture_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.ArchiveAsDeceasedAsync(1, DateTime.Now.AddDays(1)));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.ArchiveAsDeceasedAsync(1, DateTime.Now));
    }

    [TestMethod]
    public async Task ArchiveAsDeceasedAsync_WhenPatientExists_SetsDodAndArchives()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        DateTime deathDate = DateTime.Now.AddDays(-1);
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

        await service.ArchiveAsDeceasedAsync(1, deathDate);

        Assert.IsTrue(patient.IsArchived && patient.Dod == deathDate);
        _patientRepo.Verify(x => x.UpdateAsync(patient), Times.Once);
    }

    [TestMethod]
    public async Task DeletePatientAsync_WhenPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.DeletePatientAsync(1));
    }

    [TestMethod]
    public async Task DeletePatientAsync_WhenPatientExists_DeletesPatient()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());

        await service.DeletePatientAsync(1);

        _patientRepo.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task ExistsAsync_ReturnsRepositoryResult()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.ExistsAsync("1960101012345")).ReturnsAsync(true);

        bool result = await service.ExistsAsync("1960101012345");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

        Patient? result = await service.GetByIdAsync(1);

        Assert.AreSame(patient, result);
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.GetPatientDetailsAsync(1));
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenHistoryDoesNotExist_AttachesNewHistoryWithoutRecords()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        Patient result = await service.GetPatientDetailsAsync(1);

        Assert.IsTrue(result.MedicalHistory!.PatientId == 1 && result.MedicalHistory.MedicalRecords.Count == 0);
        _recordRepo.Verify(x => x.GetByHistoryIdAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task GetPatientDetailsAsync_WhenHistoryExists_LoadsDetailsAndSortsRecords()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        var history = new MedicalHistory { Id = 5, PatientId = 1 };
        var olderPrescription = new Prescription { Id = 10, RecordId = 1 };
        var newerPrescription = new Prescription { Id = 20, RecordId = 2 };
        var olderRecord = new MedicalRecord
        {
            Id = 1,
            HistoryId = 5,
            ConsultationDate = new DateTime(2024, 1, 1),
            Prescription = olderPrescription,
        };
        var newerRecord = new MedicalRecord
        {
            Id = 2,
            HistoryId = 5,
            ConsultationDate = new DateTime(2024, 2, 1),
            Prescription = newerPrescription,
        };
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _historyRepo.Setup(x => x.GetChronicConditionsAsync(5)).ReturnsAsync(new List<string> { "Asthma" });
        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(5))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)> { (new Allergy { AllergyName = "Dust" }, "mild") });
        _recordRepo.Setup(x => x.GetByHistoryIdAsync(5)).ReturnsAsync(new List<MedicalRecord> { olderRecord, newerRecord });

        Patient result = await service.GetPatientDetailsAsync(1);

        Assert.IsTrue(
            result.MedicalHistory!.ChronicConditions[0] == "Asthma"
            && result.MedicalHistory.Allergies[0].Allergy.AllergyName == "Dust"
            && result.MedicalHistory.MedicalRecords.Select(r => r.Id).SequenceEqual(new[] { 2, 1 })
            && ReferenceEquals(newerRecord, newerPrescription.MedicalRecord)
            && ReferenceEquals(olderRecord, olderPrescription.MedicalRecord));
    }

    [DataTestMethod]
    [DataRow(-1, null, null, null, "Minimum age")]
    [DataRow(null, -1, null, null, "Maximum age")]
    [DataRow(50, 20, null, null, "Minimum age cannot be greater")]
    [DataRow(null, null, "123", null, "CNP must be exactly")]
    public async Task SearchPatientsAsync_WhenFilterIsInvalid_Throws(
        int? minAge,
        int? maxAge,
        string? cnp,
        string? unused,
        string expectedMessage)
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionWithMessageAsync<ArgumentException>(
            () => service.SearchPatientsAsync(new PatientFilter
            {
                MinAge = minAge,
                MaxAge = maxAge,
                CNP = cnp,
            }),
            expectedMessage);
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenFromDateIsAfterToDate_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.SearchPatientsAsync(new PatientFilter
        {
            LastUpdatedFrom = new DateTime(2024, 2, 1),
            LastUpdatedTo = new DateTime(2024, 1, 1),
        }));
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenFilterIsValid_ReturnsRepositoryResult()
    {
        PatientService service = CreateService();
        var filter = new PatientFilter { CNP = "1960101012345" };
        var patients = new List<Patient> { CreatePatient() };
        _patientRepo.Setup(x => x.SearchAsync(filter)).ReturnsAsync(patients);

        List<Patient> result = await service.SearchPatientsAsync(filter);

        Assert.AreSame(patients, result);
    }

    [TestMethod]
    public async Task SearchPatientsAsync_WhenFilterIsNull_PassesNullToRepository()
    {
        PatientService service = CreateService();
        var patients = new List<Patient> { CreatePatient() };
        _patientRepo.Setup(x => x.SearchAsync(null!)).ReturnsAsync(patients);

        List<Patient> result = await service.SearchPatientsAsync(null!);

        Assert.AreSame(patients, result);
    }

    [TestMethod]
    public async Task IsHighRiskPatientAsync_ReturnsTrueOnlyAboveThreshold()
    {
        PatientService service = CreateService();
        _recordRepo.Setup(x => x.GetERVisitCountAsync(1, It.IsAny<DateTime>())).ReturnsAsync(11);
        _recordRepo.Setup(x => x.GetERVisitCountAsync(2, It.IsAny<DateTime>())).ReturnsAsync(10);

        Assert.IsTrue(await service.IsHighRiskPatientAsync(1) && !await service.IsHighRiskPatientAsync(2));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenPatientDoesNotExist_ThrowsBeforeNullHistoryCheck()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.CreateMedicalHistoryAsync(1, null!));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenHistoryAlreadyExists_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 5, PatientId = 1 });

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.CreateMedicalHistoryAsync(1, new MedicalHistory()));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenHistoryIsNull_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.CreateMedicalHistoryAsync(1, null!));
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenCreatedWithAllergies_SavesAllergies()
    {
        PatientService service = CreateService();
        var allergies = new List<(Allergy Allergy, string SeverityLevel)> { (new Allergy { Id = 1 }, "mild") };
        var history = new MedicalHistory { Allergies = allergies };
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepo.Setup(x => x.CreateAsync(history)).ReturnsAsync(5);

        await service.CreateMedicalHistoryAsync(1, history);

        Assert.AreEqual(1, history.PatientId);
        _historyRepo.Verify(x => x.SaveAllergiesAsync(5, It.Is<List<(Allergy Allergy, string SeverityLevel)>>(a => a.Count == 1)), Times.Once);
    }

    [TestMethod]
    public async Task CreateMedicalHistoryAsync_WhenCreatedWithoutValidIdOrAllergies_DoesNotSaveAllergies()
    {
        PatientService service = CreateService();
        var history = new MedicalHistory();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
        _historyRepo.Setup(x => x.CreateAsync(history)).ReturnsAsync(0);

        await service.CreateMedicalHistoryAsync(1, history);

        _historyRepo.Verify(x => x.SaveAllergiesAsync(It.IsAny<int>(), It.IsAny<List<(Allergy Allergy, string SeverityLevel)>>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenRecordIsNull_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => service.CreateMedicalRecordAsync(1, null!));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.CreateMedicalRecordAsync(1, new MedicalRecord()));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenHistoryDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<InvalidOperationException>(() => service.CreateMedicalRecordAsync(1, new MedicalRecord()));
    }

    [TestMethod]
    public async Task CreateMedicalRecordAsync_WhenDataIsValid_SetsHistoryAndReturnsRecordId()
    {
        PatientService service = CreateService();
        var record = new MedicalRecord();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient());
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 5, PatientId = 1 });
        _recordRepo.Setup(x => x.AddAsync(record)).ReturnsAsync(9);

        int result = await service.CreateMedicalRecordAsync(1, record);

        Assert.IsTrue(record.HistoryId == 5 && result == 9);
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenRepositoryIsMissing_Throws()
    {
        PatientService service = CreateService(includePrescriptionRepository: false);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<InvalidOperationException>(() => service.CreatePrescriptionAsync(1, new Prescription()));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenPrescriptionIsNull_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentNullException>(() => service.CreatePrescriptionAsync(1, null!));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenRecordDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((MedicalRecord?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.CreatePrescriptionAsync(1, new Prescription()));
    }

    [TestMethod]
    public async Task CreatePrescriptionAsync_WhenDataIsValid_SetsRecordAndAddsPrescription()
    {
        PatientService service = CreateService();
        var prescription = new Prescription();
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1 });

        await service.CreatePrescriptionAsync(1, prescription);

        Assert.AreEqual(1, prescription.RecordId);
        _prescriptionRepo.Verify(x => x.AddAsync(prescription), Times.Once);
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenPatientIdIsInvalid_Throws()
    {
        PatientService service = CreateService();

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.GetMedicalHistoryAsync(0));
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenRepositorySucceeds_ReturnsHistory()
    {
        PatientService service = CreateService();
        var history = new MedicalHistory { Id = 5 };
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(history);

        MedicalHistory? result = await service.GetMedicalHistoryAsync(1);

        Assert.AreSame(history, result);
    }

    [TestMethod]
    public async Task GetMedicalHistoryAsync_WhenRepositoryThrows_ReturnsNull()
    {
        PatientService service = CreateService();
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ThrowsAsync(new InvalidOperationException());

        MedicalHistory? result = await service.GetMedicalHistoryAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetMedicalRecordsAsync_WhenRepositorySucceeds_ReturnsRecords()
    {
        PatientService service = CreateService();
        var records = new List<MedicalRecord> { new MedicalRecord { Id = 1 } };
        _recordRepo.Setup(x => x.GetByHistoryIdAsync(5)).ReturnsAsync(records);

        List<MedicalRecord> result = await service.GetMedicalRecordsAsync(5);

        Assert.AreSame(records, result);
    }

    [TestMethod]
    public async Task GetMedicalRecordsAsync_WhenRepositoryThrows_ReturnsEmptyList()
    {
        PatientService service = CreateService();
        _recordRepo.Setup(x => x.GetByHistoryIdAsync(5)).ThrowsAsync(new InvalidOperationException());

        List<MedicalRecord> result = await service.GetMedicalRecordsAsync(5);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenRecordDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((MedicalRecord?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.GetRecordExportDataAsync(1));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenHistoryOrPatientDoesNotExist_Throws()
    {
        PatientService service = CreateService();
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new MedicalRecord { Id = 1, HistoryId = 5 });
        _historyRepo.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new MedicalHistory { Id = 5, Patient = null! });

        await Common.Tests.TestAssert.ThrowsExceptionAsync<KeyNotFoundException>(() => service.GetRecordExportDataAsync(1));
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPrescriptionRepositoryIsMissing_ReturnsDataWithoutPrescription()
    {
        PatientService service = CreateService(includePrescriptionRepository: false);
        Patient patient = CreatePatient();
        MedicalRecord record = new() { Id = 1, HistoryId = 5 };
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(record);
        _historyRepo.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new MedicalHistory { Id = 5, Patient = patient });

        RecordExportDataDto result = await service.GetRecordExportDataAsync(1);

        Assert.IsTrue(
            ReferenceEquals(record, result.Record)
            && ReferenceEquals(patient, result.Patient)
            && result.Prescription is null
            && result.Items.Count == 0);
    }

    [TestMethod]
    public async Task GetRecordExportDataAsync_WhenPrescriptionExists_ReturnsPrescriptionAndItems()
    {
        PatientService service = CreateService();
        Patient patient = CreatePatient();
        MedicalRecord record = new() { Id = 1, HistoryId = 5 };
        Prescription prescription = new() { Id = 9, RecordId = 1 };
        var items = new List<PrescriptionItem> { new PrescriptionItem { Id = 7 } };
        _recordRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(record);
        _historyRepo.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new MedicalHistory { Id = 5, Patient = patient });
        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1)).ReturnsAsync(prescription);
        _prescriptionRepo.Setup(x => x.GetItemsAsync(9)).ReturnsAsync(items);

        RecordExportDataDto result = await service.GetRecordExportDataAsync(1);

        Assert.IsTrue(ReferenceEquals(prescription, result.Prescription) && ReferenceEquals(items, result.Items));
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenHistoryDoesNotExist_ReturnsEmptyList()
    {
        PatientService service = CreateService();
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        List<string> result = await service.GetPatientAllergiesAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenHistoryExists_ReturnsFormattedAllergies()
    {
        PatientService service = CreateService();
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { Id = 5 });
        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(5))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)> { (new Allergy { AllergyName = "Dust" }, "mild") });

        List<string> result = await service.GetPatientAllergiesAsync(1);

        CollectionAssert.AreEqual(new[] { "Dust - mild" }, result);
    }

    [TestMethod]
    public async Task GetPatientAllergiesAsync_WhenRepositoryThrows_ReturnsEmptyList()
    {
        PatientService service = CreateService();
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ThrowsAsync(new InvalidOperationException());

        List<string> result = await service.GetPatientAllergiesAsync(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_WhenRepositoryIsMissing_Throws()
    {
        PatientService service = CreateService(includePrescriptionRepository: false);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<InvalidOperationException>(() => service.GetPrescriptionByRecordIdAsync(1));
    }

    [TestMethod]
    public async Task GetPrescriptionByRecordIdAsync_ReturnsRepositoryResult()
    {
        PatientService service = CreateService();
        Prescription prescription = new() { Id = 1 };
        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1)).ReturnsAsync(prescription);

        Prescription? result = await service.GetPrescriptionByRecordIdAsync(1);

        Assert.AreSame(prescription, result);
    }

    private static Patient CreatePatient(
        string cnp = "1960101012345",
        Sex sex = Sex.M,
        DateTime? dob = null,
        string phoneNo = "0711111111",
        bool isArchived = false)
    {
        return new Patient
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Pop",
            Cnp = cnp,
            Dob = dob ?? new DateTime(1996, 1, 1),
            Sex = sex,
            PhoneNo = phoneNo,
            EmergencyContact = "Contact",
            IsArchived = isArchived,
        };
    }

    private static SqlException CreateSqlException(int number, string message)
    {
        ConstructorInfo errorConstructor = typeof(SqlError).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types:
            [
                typeof(int),
                typeof(byte),
                typeof(byte),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(int),
                typeof(uint),
                typeof(Exception)
            ],
            modifiers: null)!;

        var error = (SqlError)errorConstructor.Invoke(
        [
            number,
            (byte)0,
            (byte)0,
            "server",
            message,
            "procedure",
            1,
            0u,
            null!
        ]);

        var errors = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true)!;
        typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(errors, [error]);

        ConstructorInfo exceptionConstructor = typeof(SqlException).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: [typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid)],
            modifiers: null)!;

        return (SqlException)exceptionConstructor.Invoke([message, errors, null!, Guid.NewGuid()]);
    }
}

