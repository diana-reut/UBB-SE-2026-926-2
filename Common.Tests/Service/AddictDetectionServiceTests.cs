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
        ChronicConditions = conditions ?? [],
        PatientAllergies = [],
    };

    private static Prescription MakePrescription(Patient patient, List<PrescriptionItem>? items = null) => new()
    {
        Id = 1,
        RecordId = 10,
        Date = DateTime.Today,
        MedicationList = items ?? [],
        MedicalRecord = new MedicalRecord
        {
            Id = 10,
            ConsultationDate = DateTime.Today,
            History = new MedicalHistory
            {
                Patient = patient,
            },
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
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([]);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasNoMedicalHistoryAssignsDefaultHistory()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNotNull(result[0].MedicalHistory);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasNoMedicalHistoryChronicConditionsIsNoneReported()
    {
        var patient = MakePatient();
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync((MedicalHistory?)null);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual("None reported.", result[0].MedicalHistory!.ChronicConditions[0]);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistoryFetchesChronicConditions()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync([]);

        await _sut.GetAddictCandidatesAsync();

        _medicalHistoryRepository.Verify(r => r.GetChronicConditionsAsync(history.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasEmptyChronicConditionsNormalizesToNoneReported()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10, conditions: []);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync([]);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual("None reported.", result[0].MedicalHistory!.ChronicConditions[0]);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistorySetsPatientNavigationToNull()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync([]);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNull(result[0].MedicalHistory!.Patient);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenPatientHasMedicalHistorySetsPatientAllergiesToNull()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id)).ReturnsAsync([]);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.IsNull(result[0].MedicalHistory!.PatientAllergies);
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
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync([]);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.BuildPoliceReportAsync(1));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionsFoundReturnsReportContainingPatientFirstName()
    {
        var patient = MakePatient();
        var prescription = MakePrescription(patient);
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([prescription]);

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains(patient.FirstName));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionsFoundReturnsReportContainingCnp()
    {
        var patient = MakePatient();
        var prescription = MakePrescription(patient);
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([prescription]);

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains(patient.Cnp));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionHasMedsReturnsMedNameInReport()
    {
        var patient = MakePatient();
        var items = new List<PrescriptionItem> { new() { MedName = "Morphine" } };
        var prescription = MakePrescription(patient, items);
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([prescription]);

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains("Morphine"));
    }

    [TestMethod]
    public async Task BuildPoliceReportAsyncWhenPrescriptionHasNoMedsReturnsUnknownInReport()
    {
        var patient = MakePatient();
        var prescription = MakePrescription(patient, items: []);
        _prescriptionRepository.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>()))
            .ReturnsAsync([prescription]);

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
        var history = MakeHistory(conditions: ["Diabetes", "Hypertension"]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("Diabetes, Hypertension", result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyFetchesFromRepository()
    {
        var history = MakeHistory(id: 10, conditions: []);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(["Asthma"]);

        await _sut.GetChronicConditionsAsync(1);

        _medicalHistoryRepository.Verify(r => r.GetChronicConditionsAsync(10), Times.Once);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyAndRepoReturnsConditionsReturnsThem()
    {
        var history = MakeHistory(id: 10, conditions: []);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync(["Asthma"]);

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("Asthma", result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsyncWhenConditionsEmptyAndRepoReturnsEmptyReturnsNoneReported()
    {
        var history = MakeHistory(id: 10, conditions: []);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(1)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(10)).ReturnsAsync([]);

        var result = await _sut.GetChronicConditionsAsync(1);

        Assert.AreEqual("None reported.", result);
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsyncWhenChronicConditionsNullAfterFetchNormalizesToNoneReported()
    {
        var patient = MakePatient();
        var history = MakeHistory(id: 10);
        _prescriptionRepository.Setup(r => r.GetAddictCandidatePatientsAsync()).ReturnsAsync([patient]);
        _medicalHistoryRepository.Setup(r => r.GetByPatientIdAsync(patient.Id)).ReturnsAsync(history);
        _medicalHistoryRepository.Setup(r => r.GetChronicConditionsAsync(history.Id))
            .ReturnsAsync((List<string>)null!);

        var result = await _sut.GetAddictCandidatesAsync();

        Assert.AreEqual("None reported.", result[0].MedicalHistory!.ChronicConditions[0]);
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
            .ReturnsAsync([prescription]);

        var result = await _sut.BuildPoliceReportAsync(1);

        Assert.IsTrue(result.Contains("Unknown"));
    }

    [TestMethod]
    public void BuildPoliceReportTextWhenPrescriptionsEmptyContainsNoMatchingRecordsText()
    {
        var method = typeof(AddictDetectionService).GetMethod(
            "BuildPoliceReportText",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = (string)method!.Invoke(null, [MakePatient(), new List<Prescription>()]);

        Assert.IsTrue(result.Contains("No matching records pulled for this timeframe."));
    }
}
