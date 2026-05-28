using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Integration.External;
using HospitalManagement.Proxy.PatientProxy;
using ERManagementSystem.Proxy.ExaminationProxy;

namespace HospitalManagement.Service;

internal class ImportService : IImportService
{
    private readonly IPatientProxy _patientService;
    private readonly IExaminationProxy _examinationProxy;
    private readonly IExternalProvider _externalAppointment;

    public ImportService(
        IPatientProxy patientService,
        IExaminationProxy examinationProxy,
        IExternalProvider externalAppointment)
    {
        _patientService = patientService;
        _examinationProxy = examinationProxy;
        _externalAppointment = externalAppointment;
    }

    public void ImportFromER(int patientId, int externalId)
    {
        ImportFromERAsync(patientId, externalId).GetAwaiter().GetResult();
    }

    public void ImportFromAppointment(int patientId, int externalId)
    {
        ImportFromAppointmentAsync(patientId, externalId).GetAwaiter().GetResult();
    }

    public async Task ImportFromERAsync(int patientId, int externalId)
    {
        Patient patient = await _patientService.GetPatientDetailsAsync(patientId);

        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        var existingErSourceIds = patient.MedicalHistory.MedicalRecords?
            .Where(record => record.SourceType == Common.Data.Entity.Enums.SourceType.ER)
            .Select(record => record.SourceId)
            .ToHashSet() ?? new HashSet<int>();

        var candidateExam = (await _examinationProxy.GetPatientHistoryAsync(patient.Cnp))
            .OrderByDescending(examination => examination.Exam_Time)
            .FirstOrDefault(examination => !existingErSourceIds.Contains(examination.Visit_ID));

        if (candidateExam is null)
        {
            throw new InvalidOperationException("No new ER examination is available to import for this patient.");
        }

        ERExaminationSummaryDto? summary = await _examinationProxy.GetSummaryByVisitIdAsync(candidateExam.Visit_ID);
        if (summary is null)
        {
            throw new InvalidOperationException("Could not load the ER examination summary for import.");
        }

        var dto = new RecordDTO
        {
            ExternalRecordId = candidateExam.Visit_ID,
            Symptoms = summary.ChiefComplaint,
            TemporaryDiagnosis = string.IsNullOrWhiteSpace(summary.Notes) ? summary.Specialization : summary.Notes,
            PrescribedMeds = string.Empty,
            ConsultationDate = summary.ExamTime,
            SourceType = Common.Data.Entity.Enums.SourceType.ER,
        };

        await ProcessImportAsync(dto, patient);
    }

    public async Task ImportFromAppointmentAsync(int patientId, int externalId)
    {
        RecordDTO dto = _externalAppointment.FetchRecordByPatientId(externalId);
        await ProcessImportAsync(dto, patientId);
    }

    private async Task ProcessImportAsync(RecordDTO dto, int patientId)
    {
        Patient patient = await _patientService.GetPatientDetailsAsync(patientId);
        await ProcessImportAsync(dto, patient);
    }

    private async Task ProcessImportAsync(RecordDTO dto, Patient patient)
    {
        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        var recordDto = BuildRecordFromDTO(dto);
        int recordId = await _patientService.CreateMedicalRecordAsync(patient.Id, recordDto);

        if (!string.IsNullOrWhiteSpace(dto.PrescribedMeds))
        {
            await CreatePrescriptionAsync(dto.PrescribedMeds, recordId);
        }
    }

    private Task CreatePrescriptionAsync(string medsString, int recordId)
    {
        string[] meds = medsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var prescription = new CreatePrescriptionDto
        {
            Date = DateTime.Now,
            DoctorNotes = "Imported from external provider",
            Items = meds.Select(m => new CreatePrescriptionItemDto
            {
                MedName = m,
                Quantity = "1",
            }).ToList(),
        };

        return _patientService.CreatePrescriptionForRecordAsync(recordId, prescription);
    }

    private static CreateMedicalRecordDto BuildRecordFromDTO(RecordDTO dto)
    {
        return new CreateMedicalRecordDto
        {
            SourceType = dto.SourceType,
            SourceId = dto.ExternalRecordId,
            StaffId = 1,
            Symptoms = dto.Symptoms,
            Diagnosis = dto.TemporaryDiagnosis,
            ConsultationDate = dto.ConsultationDate,
            BasePrice = 0,
            FinalPrice = 0,
            PoliceNotified = false,
        };
    }
}
