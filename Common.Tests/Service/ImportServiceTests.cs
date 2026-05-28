using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Models;
using ERManagementSystem.Proxy.ExaminationProxy;
using HospitalManagement.Integration.External;
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Service;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class ImportServiceTests
{
    private Mock<IPatientProxy> _patientProxy = null!;
    private Mock<IExaminationProxy> _examinationProxy = null!;
    private Mock<IExternalProvider> _externalProvider = null!;
    private ImportService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _patientProxy = new Mock<IPatientProxy>();
        _examinationProxy = new Mock<IExaminationProxy>();
        _externalProvider = new Mock<IExternalProvider>();
        _sut = new ImportService(_patientProxy.Object, _examinationProxy.Object, _externalProvider.Object);
    }

    private static Patient MakePatient(int id = 1, string cnp = "1234567890123", MedicalHistory? history = null) =>
        new()
        {
            Id = id,
            Cnp = cnp,
            FirstName = "Test",
            LastName = "Patient",
            Dob = new DateTime(1990, 1, 1),
            MedicalHistory = history,
        };

    private static MedicalHistory MakeHistory(List<MedicalRecord>? records = null) =>
        new() { MedicalRecords = records ?? new List<MedicalRecord>() };

    private static Examination MakeExam(int visitId, DateTime? examTime = null) =>
        new() { Visit_ID = visitId, Exam_Time = examTime ?? DateTime.Now };

    private static ERExaminationSummaryDto MakeSummary(
        string chiefComplaint = "Pain",
        string notes = "Observation",
        string specialization = "General") =>
        new()
        {
            ChiefComplaint = chiefComplaint,
            Notes = notes,
            Specialization = specialization,
            ExamTime = DateTime.Now,
        };

    [TestMethod]
    public async Task ImportFromERAsyncWhenPatientHasNoMedicalHistoryThrowsInvalidOperationException()
    {
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1))
            .ReturnsAsync(MakePatient(history: null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportFromERAsync(1, 0));
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenNoExaminationsExistThrowsInvalidOperationException()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp)).ReturnsAsync(new List<Examination>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportFromERAsync(1, 0));
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenAllExaminationsAlreadyImportedThrowsInvalidOperationException()
    {
        var records = new List<MedicalRecord>
        {
            new() { SourceType = SourceType.ER, SourceId = 10 },
        };
        var patient = MakePatient(history: MakeHistory(records));
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 10) });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportFromERAsync(1, 0));
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenSummaryIsNullThrowsInvalidOperationException()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 5) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(5))
            .ReturnsAsync((ERExaminationSummaryDto?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportFromERAsync(1, 0));
    }

    [TestMethod]
    public async Task ImportFromERAsyncHappyPathCreatesMedicalRecord()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 7) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(7))
            .ReturnsAsync(MakeSummary("Chest pain", "Suspected angina"));
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(99);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenNoMedsDoesNotCreatePrescription()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 7) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(7))
            .ReturnsAsync(MakeSummary("Chest pain", "Suspected angina"));
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(99);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreatePrescriptionForRecordAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionDto>()), Times.Never);
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenNotesIsEmptyUsesSpecializationAsDiagnosis()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 3) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(3))
            .ReturnsAsync(MakeSummary("Fever", notes: string.Empty, specialization: "Cardiology"));
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1,
            It.Is<CreateMedicalRecordDto>(dto => dto.Diagnosis == "Cardiology")), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenNotesIsPresentUsesNotesAsDiagnosis()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 3) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(3))
            .ReturnsAsync(MakeSummary("Fever", notes: "Suspected flu", specialization: "Cardiology"));
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1,
            It.Is<CreateMedicalRecordDto>(dto => dto.Diagnosis == "Suspected flu")), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERAsyncPicksMostRecentUnimportedExamination()
    {
        var patient = MakePatient(history: MakeHistory());
        var older = MakeExam(visitId: 10, examTime: DateTime.Now.AddDays(-2));
        var newer = MakeExam(visitId: 20, examTime: DateTime.Now.AddDays(-1));
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { older, newer });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(20)).ReturnsAsync(MakeSummary());
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _examinationProxy.Verify(p => p.GetSummaryByVisitIdAsync(20), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERAsyncSkipsAlreadyImportedExamsImportsNewOne()
    {
        var records = new List<MedicalRecord>
        {
            new() { SourceType = SourceType.ER, SourceId = 20 },
        };
        var patient = MakePatient(history: MakeHistory(records));
        var alreadyImported = MakeExam(visitId: 20, examTime: DateTime.Now.AddDays(-1));
        var newExam = MakeExam(visitId: 10, examTime: DateTime.Now.AddDays(-2));
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { alreadyImported, newExam });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(10)).ReturnsAsync(MakeSummary());
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _examinationProxy.Verify(p => p.GetSummaryByVisitIdAsync(10), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERAsyncSetsSourceTypeToER()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 1) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(1)).ReturnsAsync(MakeSummary());
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1,
            It.Is<CreateMedicalRecordDto>(dto => dto.SourceType == SourceType.ER)), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncWhenPatientHasNoMedicalHistoryThrowsInvalidOperationException()
    {
        var dto = new RecordDTO { SourceType = SourceType.App, ConsultationDate = DateTime.Now };
        _externalProvider.Setup(e => e.FetchRecordByPatientId(1)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(MakePatient(history: null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportFromAppointmentAsync(1, 1));
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncWhenNoPrescribedMedsCreatesMedicalRecord()
    {
        var dto = new RecordDTO
        {
            ExternalRecordId = 1,
            Symptoms = "Cough",
            TemporaryDiagnosis = "Common cold",
            PrescribedMeds = string.Empty,
            ConsultationDate = DateTime.Now,
            SourceType = SourceType.App,
        };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(42)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(5);

        await _sut.ImportFromAppointmentAsync(1, 42);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncWhenNoPrescribedMedsDoesNotCreatePrescription()
    {
        var dto = new RecordDTO
        {
            PrescribedMeds = string.Empty,
            ConsultationDate = DateTime.Now,
            SourceType = SourceType.App,
        };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(42)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(5);

        await _sut.ImportFromAppointmentAsync(1, 42);

        _patientProxy.Verify(p => p.CreatePrescriptionForRecordAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionDto>()), Times.Never);
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncWhenHasSingleMedCreatesPrescriptionWithOneItem()
    {
        var dto = new RecordDTO { PrescribedMeds = "Amoxicillin", SourceType = SourceType.App, ConsultationDate = DateTime.Now };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(7)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(9);

        await _sut.ImportFromAppointmentAsync(1, 7);

        _patientProxy.Verify(p => p.CreatePrescriptionForRecordAsync(9,
            It.Is<CreatePrescriptionDto>(p => p.Items.Count == 1 && p.Items[0].MedName == "Amoxicillin")), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncWhenHasMultipleMedsCreatesPrescriptionWithAllItems()
    {
        var dto = new RecordDTO { PrescribedMeds = "Amoxicillin, Ibuprofen, Paracetamol", SourceType = SourceType.App, ConsultationDate = DateTime.Now };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(7)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(9);

        await _sut.ImportFromAppointmentAsync(1, 7);

        _patientProxy.Verify(p => p.CreatePrescriptionForRecordAsync(9,
            It.Is<CreatePrescriptionDto>(p => p.Items.Count == 3)), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromAppointmentAsyncPrescriptionIsLinkedToCreatedRecord()
    {
        var dto = new RecordDTO { PrescribedMeds = "Aspirin", SourceType = SourceType.App, ConsultationDate = DateTime.Now };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(1)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(77);

        await _sut.ImportFromAppointmentAsync(1, 1);

        _patientProxy.Verify(p => p.CreatePrescriptionForRecordAsync(77, It.IsAny<CreatePrescriptionDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ImportFromERWhenPatientHasNoMedicalHistoryThrowsInvalidOperationException()
    {
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1))
            .ReturnsAsync(MakePatient(history: null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => _sut.ImportFromER(1, 0)));
    }

    [TestMethod]
    public async Task ImportFromAppointmentWhenPatientHasNoMedicalHistoryThrowsInvalidOperationException()
    {
        var dto = new RecordDTO { SourceType = SourceType.App, ConsultationDate = DateTime.Now };
        _externalProvider.Setup(e => e.FetchRecordByPatientId(1)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1))
            .ReturnsAsync(MakePatient(history: null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => _sut.ImportFromAppointment(1, 1)));
    }

    [TestMethod]
    public async Task ImportFromERAsyncWhenMedicalRecordsIsNullTreatsAsNoExistingImports()
    {
        var patient = MakePatient(history: new MedicalHistory { MedicalRecords = null });
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 1) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(1)).ReturnsAsync(MakeSummary());
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        await _sut.ImportFromERAsync(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()), Times.Once);
    }

    [TestMethod]
    public void ImportFromERHappyPathCreatesMedicalRecord()
    {
        var patient = MakePatient(history: MakeHistory());
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _examinationProxy.Setup(p => p.GetPatientHistoryAsync(patient.Cnp))
            .ReturnsAsync(new List<Examination> { MakeExam(visitId: 1) });
        _examinationProxy.Setup(p => p.GetSummaryByVisitIdAsync(1)).ReturnsAsync(MakeSummary());
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        _sut.ImportFromER(1, 0);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()), Times.Once);
    }

    [TestMethod]
    public void ImportFromAppointmentHappyPathCreatesMedicalRecord()
    {
        var dto = new RecordDTO { PrescribedMeds = string.Empty, ConsultationDate = DateTime.Now, SourceType = SourceType.App };
        var patient = MakePatient(history: MakeHistory());
        _externalProvider.Setup(e => e.FetchRecordByPatientId(1)).Returns(dto);
        _patientProxy.Setup(p => p.GetPatientDetailsAsync(1)).ReturnsAsync(patient);
        _patientProxy.Setup(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()))
            .ReturnsAsync(1);

        _sut.ImportFromAppointment(1, 1);

        _patientProxy.Verify(p => p.CreateMedicalRecordAsync(1, It.IsAny<CreateMedicalRecordDto>()), Times.Once);
    }
}
