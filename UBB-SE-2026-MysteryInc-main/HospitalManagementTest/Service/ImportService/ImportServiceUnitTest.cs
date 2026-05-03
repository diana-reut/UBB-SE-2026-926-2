using Moq;
using HospitalManagement.Entity;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using HospitalManagement.Integration.External;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class ImportServiceUnitTests
{
    private Mock<IPatientService> _patientServiceMock;
    private Mock<IMedicalRecordRepository> _recordRepoMock;
    private Mock<IPrescriptionRepository> _prescriptionRepoMock;
    private Mock<IExternalProvider> _externalERMock;
    private Mock<IExternalProvider> _externalAppointmentMock;
    private ImportService _sut;

    [TestInitialize]
    public void Setup()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _recordRepoMock = new Mock<IMedicalRecordRepository>();
        _prescriptionRepoMock = new Mock<IPrescriptionRepository>();
        _externalERMock = new Mock<IExternalProvider>();
        _externalAppointmentMock = new Mock<IExternalProvider>();

        _sut = new ImportService(
            _patientServiceMock.Object,
            _recordRepoMock.Object,
            _prescriptionRepoMock.Object,
            _externalERMock.Object,
            _externalAppointmentMock.Object);
    }

    [TestMethod]
    public void ImportFromER_ShouldFetchFromExternalER_AndProcessImport()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = BuildPatientWithHistory();

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(99))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        _externalERMock.Verify(p => p.FetchRecordByPatientId(99), Times.Once);
        _recordRepoMock.Verify(r => r.Add(It.IsAny<MedicalRecord>()), Times.Once);
    }

    [TestMethod]
    public void ImportFromER_ShouldNotUseAppointmentProvider()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = BuildPatientWithHistory();

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(It.IsAny<int>()))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        _externalAppointmentMock.Verify(p => p.FetchRecordByPatientId(It.IsAny<int>()), Times.Never);
    }    

    [TestMethod]
    public void ImportFromAppointment_ShouldFetchFromAppointmentProvider_AndProcessImport()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = BuildPatientWithHistory();

        _externalAppointmentMock
            .Setup(p => p.FetchRecordByPatientId(42))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromAppointment(patientId: 1, externalId: 42);

        _externalAppointmentMock.Verify(p => p.FetchRecordByPatientId(42), Times.Once);
        _recordRepoMock.Verify(r => r.Add(It.IsAny<MedicalRecord>()), Times.Once);
    }

    [TestMethod]
    public void ImportFromAppointment_ShouldNotUseERProvider()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = BuildPatientWithHistory();

        _externalAppointmentMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(It.IsAny<int>()))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromAppointment(patientId: 1, externalId: 42);

        _externalERMock.Verify(p => p.FetchRecordByPatientId(It.IsAny<int>()), Times.Never);
    }    

    [TestMethod]
    public void ProcessImport_WhenMedicalHistoryIsNull_ShouldThrowInvalidOperationException()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = new Patient { Id = 1, MedicalHistory = null };

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);

        Assert.Throws<InvalidOperationException>(
            () => _sut.ImportFromER(patientId: 1, externalId: 99));
    }

    [TestMethod]
    public void ProcessImport_WhenMedicalHistoryIsNull_ShouldNotAddRecord()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = new Patient { Id = 1, MedicalHistory = null };

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);

        _ = Assert.Throws<InvalidOperationException>(
            () => _sut.ImportFromER(patientId: 1, externalId: 99));

        _recordRepoMock.Verify(r => r.Add(It.IsAny<MedicalRecord>()), Times.Never);
    }    

    [TestMethod]
    public void ProcessImport_WhenPrescribedMedsIsEmpty_ShouldNotCreatePrescription()
    {
        var dto = BuildDto(prescribedMeds: "");
        var patient = BuildPatientWithHistory();

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        _prescriptionRepoMock.Verify(r => r.Add(It.IsAny<Prescription>()), Times.Never);
    }

    [TestMethod]
    public void ProcessImport_WhenPrescribedMedsIsWhitespace_ShouldNotCreatePrescription()
    {
        var dto = BuildDto(prescribedMeds: "   ");
        var patient = BuildPatientWithHistory();

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        _prescriptionRepoMock.Verify(r => r.Add(It.IsAny<Prescription>()), Times.Never);
    }

    [TestMethod]
    public void ProcessImport_WhenPrescribedMedsIsPresent_ShouldCreatePrescription()
    {
        var dto = BuildDto(prescribedMeds: "Aspirin, Ibuprofen");
        var patient = BuildPatientWithHistory();

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        _prescriptionRepoMock.Verify(r => r.Add(It.IsAny<Prescription>()), Times.Once);
    }

    [TestMethod]
    public void ProcessImport_WhenPrescribedMedsIsPresent_ShouldMapEachMedToAPrescriptionItem()
    {
        var dto = BuildDto(prescribedMeds: "Aspirin, Ibuprofen, Paracetamol");
        var patient = BuildPatientWithHistory();
        Prescription? captured = null;

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(10);
        _prescriptionRepoMock
            .Setup(r => r.Add(It.IsAny<Prescription>()))
            .Callback<Prescription>(p => captured = p);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        Assert.IsNotNull(captured);
        Assert.AreEqual(3, captured.MedicationList.Count);
        Assert.IsTrue(captured.MedicationList.Any(m => m.MedName == "Aspirin"));
        Assert.IsTrue(captured.MedicationList.Any(m => m.MedName == "Ibuprofen"));
        Assert.IsTrue(captured.MedicationList.Any(m => m.MedName == "Paracetamol"));
    }

    [TestMethod]
    public void ProcessImport_WhenPrescribedMedsIsPresent_ShouldLinkPrescriptionToRecord()
    {
        var dto = BuildDto(prescribedMeds: "Aspirin");
        var patient = BuildPatientWithHistory();
        Prescription? captured = null;

        _externalERMock
            .Setup(p => p.FetchRecordByPatientId(It.IsAny<int>()))
            .Returns(dto);
        _patientServiceMock
            .Setup(s => s.GetPatientDetails(1))
            .Returns(patient);
        _recordRepoMock
            .Setup(r => r.Add(It.IsAny<MedicalRecord>()))
            .Returns(55);
        _prescriptionRepoMock
            .Setup(r => r.Add(It.IsAny<Prescription>()))
            .Callback<Prescription>(p => captured = p);

        _sut.ImportFromER(patientId: 1, externalId: 99);

        Assert.IsNotNull(captured);
        Assert.AreEqual(55, captured.RecordId);
    }    

    private static RecordDTO BuildDto(string prescribedMeds) => new()
    {
        ExternalRecordId = 1,
        Symptoms = "Fever",
        TemporaryDiagnosis = "Flu",
        PrescribedMeds = prescribedMeds,
        ConsultationDate = DateTime.Today,
    };

    private static Patient BuildPatientWithHistory() => new()
    {
        Id = 1,
        MedicalHistory = new MedicalHistory { Id = 7 },
    };
}