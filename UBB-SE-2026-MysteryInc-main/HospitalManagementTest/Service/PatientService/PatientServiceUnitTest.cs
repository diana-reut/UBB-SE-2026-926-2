using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using Moq;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class PatientServiceUnitTests
{

    private Mock<IPatientRepository> _patientRepoMock = null!;
    private Mock<IMedicalHistoryRepository> _historyRepoMock = null!;
    private Mock<IMedicalRecordRepository> _recordRepoMock = null!;
    private Mock<IPrescriptionRepository> _prescriptionRepoMock = null!;
    private PatientService _sut = null!;

    private static readonly DateTime ValidDob = new(1990, 5, 15);
    private const string ValidMaleCnp = "1900515000001";
    private const string ValidFemaleCnp = "2900515000001";
    private const string ValidPhoneNumber = "0770921659";

    [TestInitialize]
    public void Setup()
    {
        _patientRepoMock = new Mock<IPatientRepository>();
        _historyRepoMock = new Mock<IMedicalHistoryRepository>();
        _recordRepoMock = new Mock<IMedicalRecordRepository>();
        _prescriptionRepoMock = new Mock<IPrescriptionRepository>();

        _sut = new PatientService(
            _patientRepoMock.Object,
            _historyRepoMock.Object,
            _recordRepoMock.Object,
            _prescriptionRepoMock.Object);
    }

    [TestMethod]
    public void ValidateCNP_NullCnp_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP(null!, Sex.M, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_WhitespaceCnp_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("   ", Sex.M, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_WrongLength_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("123456", Sex.M, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_ContainsNonDigit_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP("190051500000A", Sex.M, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_MaleWithEvenFirstDigit_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP(ValidFemaleCnp, Sex.M, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_FemaleWithOddFirstDigit_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP(ValidMaleCnp, Sex.F, ValidDob);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_DobMismatch_ReturnsFalse()
    {
        bool result = _sut.ValidateCNP(ValidMaleCnp, Sex.M, new DateTime(1990, 6, 15));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCNP_ValidMale_ReturnsTrue()
    {
        bool result = _sut.ValidateCNP(ValidMaleCnp, Sex.M, ValidDob);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ValidateCNP_ValidFemale_ReturnsTrue()
    {
        bool result = _sut.ValidateCNP(ValidFemaleCnp, Sex.F, ValidDob);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CreatePatient_NullData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.CreatePatient(null!));
    }

    [TestMethod]
    public void CreatePatient_FutureDob_ThrowsArgumentException()
    {
        var patient = new Patient { Dob = DateTime.Today.AddDays(1), Cnp = ValidMaleCnp, Sex = Sex.M };
        Assert.Throws<ArgumentException>(() => _sut.CreatePatient(patient));
    }

    [TestMethod]
    public void CreatePatient_TodayAsDob_ThrowsArgumentException()
    {
        var patient = new Patient { Dob = DateTime.Today, Cnp = ValidMaleCnp, Sex = Sex.M };
        Assert.Throws<ArgumentException>(() => _sut.CreatePatient(patient));
    }

    [TestMethod]
    public void CreatePatient_InvalidCnp_ThrowsArgumentException()
    {
        var patient = new Patient { Dob = ValidDob, Cnp = ValidFemaleCnp, Sex = Sex.M };
        Assert.Throws<ArgumentException>(() => _sut.CreatePatient(patient));
    }

    [TestMethod]
    public void CreatePatient_ValidData_AddsAndReturnsPatient()
    {
        var patient = new Patient { Dob = ValidDob, Cnp = ValidMaleCnp, Sex = Sex.M };

        Patient result = _sut.CreatePatient(patient);

        _patientRepoMock.Verify(r => r.Add(patient), Times.Once);
        Assert.AreEqual(patient, result);
    }

    [TestMethod]
    public void UpdatePatient_NullData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.UpdatePatient(null!));
    }

    [TestMethod]
    public void UpdatePatient_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);
        var patient = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M };

        Assert.Throws<KeyNotFoundException>(() => _sut.UpdatePatient(patient));
    }

    [TestMethod]
    public void UpdatePatient_CnpChanged_ThrowsInvalidOperationException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = "1900515000002", Dob = ValidDob, Sex = Sex.M };

        Assert.Throws<InvalidOperationException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_DobChanged_ThrowsInvalidOperationException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob.AddDays(1), Sex = Sex.M };

        Assert.Throws<InvalidOperationException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_CnpSexMismatch_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.F };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_NullPhoneNo_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = null! };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_EmptyPhoneNo_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "" };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_PhoneNoWrongLength_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "12345" };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_PhoneNoContainsLetters_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "123456789A" };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_WhiteSpacesPhoneNumber_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "   " };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_ValidPhoneNo_DoesNotThrow()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "0712345678" };

        _sut.UpdatePatient(updated);

        _patientRepoMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Once);
    }

    [TestMethod]
    public void UpdatePatient_PhoneNoWithSpecialChar_ThrowsArgumentException()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);

        var updated = new Patient
        {
            Id = 1,
            Cnp = ValidMaleCnp,
            Dob = ValidDob,
            Sex = Sex.M,
            PhoneNo = "07123.4567"
        };

        Assert.Throws<ArgumentException>(() => _sut.UpdatePatient(updated));
    }

    [TestMethod]
    public void UpdatePatient_ValidData_CallsRepositoryUpdate()
    {
        var existing = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(existing);
        var updated = new Patient { Id = 1, Cnp = ValidMaleCnp, Dob = ValidDob, Sex = Sex.M, PhoneNo = "0712345678" };

        _sut.UpdatePatient(updated);

        _patientRepoMock.Verify(r => r.Update(updated), Times.Once);
    }

    [TestMethod]
    public void ArchivePatient_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(() => _sut.ArchivePatient(1));
    }

    [TestMethod]
    public void ArchivePatient_PatientFound_SetsIsArchivedTrueAndCallsUpdate()
    {
        var patient = new Patient { Id = 1, IsArchived = false };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(patient);

        _sut.ArchivePatient(1);

        Assert.IsTrue(patient.IsArchived);
        _patientRepoMock.Verify(r => r.Update(patient), Times.Once);
    }

    [TestMethod]
    public void DearchivePatient_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(() => _sut.DearchivePatient(1));
    }

    [TestMethod]
    public void DearchivePatient_PatientFound_ClearsIsArchivedAndCallsUpdate()
    {
        var patient = new Patient { Id = 1, IsArchived = true };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(patient);

        _sut.DearchivePatient(1);

        Assert.IsFalse(patient.IsArchived);
        _patientRepoMock.Verify(r => r.Update(patient), Times.Once);
    }

    [TestMethod]
    public void ArchiveAsDeceased_FutureDeathDate_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.ArchiveAsDeceased(1, DateTime.Now.AddDays(1)));
    }

    [TestMethod]
    public void ArchiveAsDeceased_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(
            () => _sut.ArchiveAsDeceased(1, DateTime.Now.AddDays(-1)));
    }

    [TestMethod]
    public void ArchiveAsDeceased_ValidData_SetsArchivedDodAndCallsUpdate()
    {
        var patient = new Patient { Id = 1 };
        var deathDate = DateTime.Now.AddDays(-1);
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(patient);

        _sut.ArchiveAsDeceased(1, deathDate);

        Assert.IsTrue(patient.IsArchived);
        Assert.AreEqual(deathDate, patient.Dod);
        _patientRepoMock.Verify(r => r.Update(patient), Times.Once);
    }

    [TestMethod]
    public void SearchPatients_NullFilter_SkipsValidationAndCallsSearch()
    {
        _patientRepoMock.Setup(r => r.Search(It.IsAny<PatientFilter>())).Returns([]);

        List<Patient> result = _sut.SearchPatients(null!);

        _patientRepoMock.Verify(r => r.Search(It.IsAny<PatientFilter>()), Times.Once);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void SearchPatients_NegativeMinAge_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.SearchPatients(new PatientFilter { MinAge = -1 }));
    }

    [TestMethod]
    public void SearchPatients_NegativeMaxAge_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.SearchPatients(new PatientFilter { MaxAge = -1 }));
    }

    [TestMethod]
    public void SearchPatients_MinAgeGreaterThanMaxAge_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.SearchPatients(new PatientFilter { MinAge = 50, MaxAge = 20 }));
    }

    [TestMethod]
    public void SearchPatients_CnpWrongLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.SearchPatients(new PatientFilter { CNP = "123" }));
    }

    [TestMethod]
    public void SearchPatients_InvalidDateRange_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => _sut.SearchPatients(new PatientFilter
            {
                LastUpdatedFrom = DateTime.Now,
                LastUpdatedTo = DateTime.Now.AddDays(-1)
            }));
    }

    [TestMethod]
    public void SearchPatients_ValidAgeRange_CallsSearch()
    {
        _patientRepoMock.Setup(r => r.Search(It.IsAny<PatientFilter>())).Returns([]);
        var filter = new PatientFilter { MinAge = 10, MaxAge = 50 };

        _sut.SearchPatients(filter);

        _patientRepoMock.Verify(r => r.Search(filter), Times.Once);
    }

    [TestMethod]
    public void SearchPatients_ValidCnp_CallsSearch()
    {
        var patients = new List<Patient> { new() { Id = 1 } };
        _patientRepoMock.Setup(r => r.Search(It.IsAny<PatientFilter>())).Returns(patients);
        var filter = new PatientFilter { CNP = "1234567890123" };   

        List<Patient> result = _sut.SearchPatients(filter);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void SearchPatients_ValidDateRange_CallsSearch()
    {
        _patientRepoMock.Setup(r => r.Search(It.IsAny<PatientFilter>())).Returns([]);
        var filter = new PatientFilter
        {
            LastUpdatedFrom = DateTime.Now.AddDays(-1),
            LastUpdatedTo = DateTime.Now
        };

        _sut.SearchPatients(filter);

        _patientRepoMock.Verify(r => r.Search(filter), Times.Once);
    }

    [TestMethod]
    public void CreateMedicalHistory_PatientNotFound_ThrowsArgumentException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<ArgumentException>(
            () => _sut.CreateMedicalHistory(1, new MedicalHistory()));
    }

    [TestMethod]
    public void CreateMedicalHistory_HistoryAlreadyExists_ThrowsArgumentException()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1))
                        .Returns(new MedicalHistory { Id = 1, PatientId = 1 });

        Assert.Throws<ArgumentException>(
            () => _sut.CreateMedicalHistory(1, new MedicalHistory()));
    }

    [TestMethod]
    public void CreateMedicalHistory_NullHistory_ThrowsArgumentException()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);

        Assert.Throws<ArgumentException>(
            () => _sut.CreateMedicalHistory(1, null!));
    }

    [TestMethod]
    public void CreateMedicalHistory_WithAllergiesAndPositiveId_CallsSaveAllergies()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);
        _historyRepoMock.Setup(r => r.Create(It.IsAny<MedicalHistory>())).Returns(5);

        var allergy = new Allergy { AllergyName = "Peanuts" };
        var history = new MedicalHistory { Allergies = [(allergy, "Severe")] };

        _sut.CreateMedicalHistory(1, history);

        _historyRepoMock.Verify(r => r.SaveAllergies(5, history.Allergies), Times.Once);
    }

    [TestMethod]
    public void CreateMedicalHistory_EmptyAllergies_DoesNotCallSaveAllergies()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);
        _historyRepoMock.Setup(r => r.Create(It.IsAny<MedicalHistory>())).Returns(5);

        var history = new MedicalHistory { Allergies = [] };

        _sut.CreateMedicalHistory(1, history);

        _historyRepoMock.Verify(
            r => r.SaveAllergies(It.IsAny<int>(), It.IsAny<List<(Allergy, string)>>()),
            Times.Never);
    }

    [TestMethod]
    public void CreateMedicalHistory_NullAllergies_DoesNotCallSaveAllergies()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);
        _historyRepoMock.Setup(r => r.Create(It.IsAny<MedicalHistory>())).Returns(5);

        var history = new MedicalHistory { Allergies = null! };

        _sut.CreateMedicalHistory(1, history);

        _historyRepoMock.Verify(
            r => r.SaveAllergies(It.IsAny<int>(), It.IsAny<List<(Allergy, string)>>()),
            Times.Never);
    }

    [TestMethod]
    public void CreateMedicalHistory_CreateReturnsZero_DoesNotCallSaveAllergies()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);
        _historyRepoMock.Setup(r => r.Create(It.IsAny<MedicalHistory>())).Returns(0);

        var allergy = new Allergy { AllergyName = "Peanuts" };
        var history = new MedicalHistory { Allergies = [(allergy, "Severe")] };

        _sut.CreateMedicalHistory(1, history);

        _historyRepoMock.Verify(
            r => r.SaveAllergies(It.IsAny<int>(), It.IsAny<List<(Allergy, string)>>()),
            Times.Never);
    }

    [TestMethod]
    public void GetPatientDetails_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(() => _sut.GetPatientDetails(1));
    }

    [TestMethod]
    public void GetPatientDetails_NoHistory_AssignsEmptyHistoryWithPatientId()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);

        Patient result = _sut.GetPatientDetails(1);

        Assert.IsNotNull(result.MedicalHistory);
        Assert.AreEqual(1, result.MedicalHistory.PatientId);
        Assert.AreEqual(0, result.MedicalHistory.Id);   
    }

    [TestMethod]
    public void GetPatientDetails_HistoryFoundWithZeroId_DoesNotFetchRecords()
    {
        var history = new MedicalHistory { Id = 0, PatientId = 1 };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns(history);
        _historyRepoMock.Setup(r => r.GetChronicConditions(0)).Returns([]);
        _historyRepoMock.Setup(r => r.GetAllergiesByHistoryId(0)).Returns([]);

        Patient result = _sut.GetPatientDetails(1);

        _recordRepoMock.Verify(r => r.GetByHistoryId(It.IsAny<int>()), Times.Never);
        Assert.IsNotNull(result.MedicalHistory);
    }

    [TestMethod]
    public void GetPatientDetails_HistoryFoundWithPositiveId_FetchesAndOrdersRecords()
    {
        var older = new MedicalRecord { Id = 1, HistoryId = 5, ConsultationDate = DateTime.Now.AddDays(-2) };
        var newer = new MedicalRecord { Id = 2, HistoryId = 5, ConsultationDate = DateTime.Now };
        var history = new MedicalHistory { Id = 5, PatientId = 1 };

        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns(history);
        _historyRepoMock.Setup(r => r.GetChronicConditions(5)).Returns(["Diabetes"]);
        _historyRepoMock.Setup(r => r.GetAllergiesByHistoryId(5)).Returns([]);
        _recordRepoMock.Setup(r => r.GetByHistoryId(5)).Returns([older, newer]);

        Patient result = _sut.GetPatientDetails(1);

        _recordRepoMock.Verify(r => r.GetByHistoryId(5), Times.Once);
        Assert.HasCount(2, result.MedicalHistory!.MedicalRecords);
        Assert.AreEqual(newer.Id, result.MedicalHistory.MedicalRecords[0].Id);
        Assert.AreEqual(older.Id, result.MedicalHistory.MedicalRecords[1].Id);
    }

    [TestMethod]
    public void IsHighRiskPatient_MoreThanTenVisits_ReturnsTrue()
    {
        _recordRepoMock.Setup(r => r.GetERVisitCount(1, It.IsAny<DateTime>())).Returns(11);

        Assert.IsTrue(_sut.IsHighRiskPatient(1));
    }

    [TestMethod]
    public void IsHighRiskPatient_ExactlyTenVisits_ReturnsFalse()
    {
        _recordRepoMock.Setup(r => r.GetERVisitCount(1, It.IsAny<DateTime>())).Returns(10);

        Assert.IsFalse(_sut.IsHighRiskPatient(1));
    }

    [TestMethod]
    public void IsHighRiskPatient_FewerThanTenVisits_ReturnsFalse()
    {
        _recordRepoMock.Setup(r => r.GetERVisitCount(1, It.IsAny<DateTime>())).Returns(3);

        Assert.IsFalse(_sut.IsHighRiskPatient(1));
    }

    [TestMethod]
    public void DeletePatient_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(() => _sut.DeletePatient(1));
    }

    [TestMethod]
    public void DeletePatient_PatientFound_CallsDelete()
    {
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });

        _sut.DeletePatient(1);

        _patientRepoMock.Verify(r => r.Delete(1), Times.Once);
    }

    [TestMethod]
    public void Exists_RepoReturnsTrue_ReturnsTrue()
    {
        _patientRepoMock.Setup(r => r.Exists("1234567890123")).Returns(true);

        Assert.IsTrue(_sut.Exists("1234567890123"));
    }

    [TestMethod]
    public void Exists_RepoReturnsFalse_ReturnsFalse()
    {
        _patientRepoMock.Setup(r => r.Exists("1234567890123")).Returns(false);

        Assert.IsFalse(_sut.Exists("1234567890123"));
    }

    [TestMethod]
    public void GetMedicalHistory_ZeroId_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _sut.GetMedicalHistory(0));
    }

    [TestMethod]
    public void GetMedicalHistory_NegativeId_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _sut.GetMedicalHistory(-5));
    }

    [TestMethod]
    public void GetMedicalHistory_ValidId_ReturnsHistory()
    {
        var history = new MedicalHistory { Id = 1, PatientId = 1 };
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns(history);

        MedicalHistory? result = _sut.GetMedicalHistory(1);

        Assert.AreEqual(history, result);
    }

    [TestMethod]
    public void GetMedicalHistory_RepoThrows_ReturnsNull()
    {
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Throws(new Exception("DB error"));

        MedicalHistory? result = _sut.GetMedicalHistory(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetMedicalRecords_ValidHistoryId_ReturnsRecordList()
    {
        var records = new List<MedicalRecord> { new() { Id = 1 } };
        _recordRepoMock.Setup(r => r.GetByHistoryId(1)).Returns(records);

        List<MedicalRecord> result = _sut.GetMedicalRecords(1);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetMedicalRecords_RepoThrows_ReturnsEmptyList()
    {
        _recordRepoMock.Setup(r => r.GetByHistoryId(1)).Throws(new Exception("DB error"));

        List<MedicalRecord> result = _sut.GetMedicalRecords(1);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetPatientAllergies_NoHistory_ReturnsEmptyList()
    {
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory?)null);

        List<string> result = _sut.GetPatientAllergies(1);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetPatientAllergies_HistoryExists_ReturnsFormattedStrings()
    {
        var history = new MedicalHistory { Id = 1, PatientId = 1 };
        var allergy = new Allergy { AllergyName = "Peanuts" };
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Returns(history);
        _historyRepoMock.Setup(r => r.GetAllergiesByHistoryId(1))
                        .Returns([(allergy, "Severe")]);

        List<string> result = _sut.GetPatientAllergies(1);

        Assert.HasCount(1, result);
        Assert.AreEqual("Peanuts - Severe", result[0]);
    }

    [TestMethod]
    public void GetPatientAllergies_RepoThrows_ReturnsEmptyList()
    {
        _historyRepoMock.Setup(r => r.GetByPatientId(1)).Throws(new Exception("DB error"));

        List<string> result = _sut.GetPatientAllergies(1);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetPrescriptionByRecordId_NoPrescriptionRepo_ThrowsInvalidOperationException()
    {
        var sut = new PatientService(
            _patientRepoMock.Object,
            _historyRepoMock.Object,
            _recordRepoMock.Object,
            prescriptionRepo: null);

        Assert.Throws<InvalidOperationException>(
            () => sut.GetPrescriptionByRecordId(1));
    }

    [TestMethod]
    public void GetPrescriptionByRecordId_RepoAvailable_ReturnsPrescription()
    {
        var prescription = new Prescription { Id = 1, RecordId = 1 };
        _prescriptionRepoMock.Setup(r => r.GetByRecordId(1)).Returns(prescription);

        Prescription? result = _sut.GetPrescriptionByRecordId(1);

        Assert.AreEqual(prescription, result);
    }

    [TestMethod]
    public void GetById_PatientNotFound_ThrowsKeyNotFoundException()
    {
        _patientRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Patient?)null);

        Assert.Throws<KeyNotFoundException>(() => _sut.GetById(1));
    }

    [TestMethod]
    public void GetById_PatientFound_ReturnsPatient()
    {
        var patient = new Patient { Id = 1 };
        _patientRepoMock.Setup(r => r.GetById(1)).Returns(patient);

        Patient result = _sut.GetById(1);

        Assert.AreEqual(patient, result);
    }
}