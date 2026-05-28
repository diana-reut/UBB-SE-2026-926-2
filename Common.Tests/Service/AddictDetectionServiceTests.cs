using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Common.Data.Repository;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class AddictDetectionServiceTests
{
    private Mock<IPrescriptionRepository> _prescriptionRepository = null!;
    private Mock<IMedicalHistoryRepository> _medicalHistoryRepository = null!;
    private AddictDetectionService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _prescriptionRepository = new Mock<IPrescriptionRepository>();
        _medicalHistoryRepository = new Mock<IMedicalHistoryRepository>();
        _prescriptionRepository
            .Setup(r => r.GetPoliceNotifiedPatientIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<int>());
        _sut = new AddictDetectionService(_prescriptionRepository.Object, _medicalHistoryRepository.Object);
    }

    private static Patient MakePatient(int id = 1) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Doe",
        Cnp = "1234567890123",
        PhoneNo = "0700000000",
        EmergencyContact = "John Doe",
        Dob = new DateTime(1990, 1, 1),
        Sex = Sex.F,
    };

    private static MedicalHistory MakeHistory(int id = 10, List<string>? conditions = null) => new()
    {
        Id = id,
        PatientId = 1,
        Patient = MakePatient(),
        ChronicConditions = conditions ?? new List<string>(),
        PatientAllergies = new List<PatientAllergy>(),
    };

    private static Prescription MakePrescription(Patient patient, List<PrescriptionItem>? items = null) => new()
    {
        Id = 1,
        RecordId = 10,
        Date = DateTime.Today,
        MedicationList = items ?? new List<PrescriptionItem>(),
        MedicalRecord = new MedicalRecord
        {
            Id = 10,
            ConsultationDate = DateTime.Today,
            History = new MedicalHistory { Patient = patient },
        },
    };

    [TestMethod]
    public void ConstructorWhenPrescriptionRepositoryIsNullThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AddictDetectionService(null!, _medicalHistoryRepository.Object));
    }

    [TestMethod]
    public void ConstructorWhenMedicalHistoryRepositoryIsNullThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AddictDetectionService(_prescriptionRepository.Object, null!));
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenNoPatientsReturnedReturnsEmptyList()
    {
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient>());

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasNoMedicalHistoryAssignsDefaultHistory()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNotNull(result[0].MedicalHistory);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasNoMedicalHistoryChronicConditionsIsNoneReported()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual("None reported.", result[0].MedicalHistory!.ChronicConditions[0]);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistoryFetchesChronicConditions()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync(new List<string>());

        await _sut.GetAddictCandidatesAsync();

        _medicalHistoryRepository.Verify(r => r.GetChronicConditionsAsync(history.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasEmptyChronicConditionsNormalizesToNoneReported()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10, conditions: new List<string>());
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync(new List<string>());

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual("None reported.", result[0].MedicalHistory!.ChronicConditions[0]);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistorySetsPatientNavigationToNull()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync(new List<string>());

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNull(result[0].MedicalHistory!.Patient);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistorySetsPatientAllergiesToNull()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync(new List<string>());

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNull(result[0].MedicalHistory!.PatientAllergies);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientIdInNotifiedListSetsIsPoliceNotifiedTrue()
    {
        var patient = MakePatient(id: 5);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _prescriptionRepository
            .Setup(r => r.GetPoliceNotifiedPatientIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<int> { 5 });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsTrue(result[0].IsPoliceNotified);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientIdNotInNotifiedListSetsIsPoliceNotifiedFalse()
    {
        var patient = MakePatient(id: 5);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync(new List<Patient> { patient });
        _prescriptionRepository
            .Setup(r => r.GetPoliceNotifiedPatientIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<int> { 99 });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsFalse(result[0].IsPoliceNotified);
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsyncWhenPatientIdIsZeroThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.MarkPoliceNotifiedAsync(0));
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsyncWhenPatientIdIsNegativeThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.MarkPoliceNotifiedAsync(-1));
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsyncWhenValidIdDelegatesCallToRepository()
    {
        _prescriptionRepository.Setup(r => r.MarkPoliceNotifiedAsync(7)).Returns(Task.CompletedTask);

        await _sut.MarkPoliceNotifiedAsync(7);

        _prescriptionRepository.Verify(r => r.MarkPoliceNotifiedAsync(7), Times.Once);
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPatientIdIsZeroThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.BuildPoliceReportAsync(0));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPatientIdIsNegativeThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.BuildPoliceReportAsync(-1));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenNoPrescriptionsFoundThrowsArgumentException()
    {
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription>());

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.BuildPoliceReportAsync(1));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionsFoundReturnsReportContainingPatientFirstName()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync(new List<Prescription> { MakePrescription(patient) });

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains(patient.FirstName));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionsFoundReturnsReportContainingCnp()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync(new List<Prescription> { MakePrescription(patient) });

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains(patient.Cnp));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionHasMedsReturnsMedNameInReport()
    {
        var patient = MakePatient();
        var items = new List<PrescriptionItem> { new() { MedName = "Morphine" } };
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync(new List<Prescription> { MakePrescription(patient, items) });

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains("Morphine"));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionHasNoMedsReturnsUnknownInReport()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync(new List<Prescription> { MakePrescription(patient, items: new List<PrescriptionItem>()) });

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains("Unknown"));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenMedicationListIsNullReturnsUnknownInReport()
    {
        var patient = MakePatient();
        var prescription = new Prescription
        {
            Id = 1,
            RecordId = 10,
            Date = DateTime.Today,
            MedicationList = null!,
            MedicalRecord = new MedicalRecord
            {
                Id = 10,
                ConsultationDate = DateTime.Today,
                History = new MedicalHistory { Patient = patient },
            },
        };
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync(new List<Prescription> { prescription });

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains("Unknown"));
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenPatientIdIsZeroThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetChronicConditionsAsync(0));
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenPatientIdIsNegativeThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetChronicConditionsAsync(-5));
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenHistoryIsNullReturnsNoneReported()
    {
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("None reported.", result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsAlreadyLoadedReturnsThem()
    {
        var history = MakeHistory(conditions: new List<string> { "Diabetes", "Hypertension" });
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("Diabetes, Hypertension", result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsNullFetchesFromRepository()
    {
        var history = new MedicalHistory { Id = 10, PatientId = 1, ChronicConditions = null! };
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(new List<string> { "Asthma" });

        await _sut.GetChronicConditionsAsync(1);

        _medicalHistoryRepository.Verify(r => r.GetChronicConditionsAsync(10), Times.Once);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyFetchesFromRepository()
    {
        var history = MakeHistory(id: 10, conditions: new List<string>());
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(new List<string> { "Asthma" });

        await _sut.GetChronicConditionsAsync(1);

        _medicalHistoryRepository.Verify(r => r.GetChronicConditionsAsync(10), Times.Once);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyAndRepoReturnsConditionsReturnsThem()
    {
        var history = MakeHistory(id: 10, conditions: new List<string>());
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(new List<string> { "Asthma" });

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("Asthma", result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyAndRepoReturnsEmptyReturnsNoneReported()
    {
        var history = MakeHistory(id: 10, conditions: new List<string>());
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(new List<string>());

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("None reported.", result);
    }

    [TestMethod]
    public void NormalizeMedicalHistoryWhenChronicConditionsIsNullSetsToNoneReported()
    {
        var patient = MakePatient();
        patient.MedicalHistory = new MedicalHistory { ChronicConditions = null! };

        var method = typeof(AddictDetectionService).GetMethod(
            "NormalizeMedicalHistory",
            BindingFlags.NonPublic | BindingFlags.Static);
        method!.Invoke(null, new object?[] { patient });

        Assert.AreEqual("None reported.", patient.MedicalHistory.ChronicConditions[0]);
    }

    [TestMethod]
    public void BuildPoliceReportTextWhenPrescriptionsEmptyContainsNoMatchingRecordsText()
    {
        var method = typeof(AddictDetectionService).GetMethod(
            "BuildPoliceReportText",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = (string)method!.Invoke(null, new object?[] { MakePatient(), new List<Prescription>() });

        Assert.IsTrue(result.Contains("No matching records pulled for this timeframe."));
    }
}
