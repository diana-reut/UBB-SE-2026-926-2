using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Repository;
using HospitalManagement.Integration.External;
using HospitalManagement.Proxy.PatientProxy;


namespace HospitalManagement.Service;

internal class ImportService : IImportService
{
    private readonly IPatientProxy _patientService;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository _prescriptionRepo;
    private readonly IExternalProvider _externalER;
    private readonly IExternalProvider _externalAppointment;

    public ImportService(
        IPatientProxy patientService,
        IMedicalRecordRepository recordRepo,
        IPrescriptionRepository prescriptionRepo,
        IExternalProvider externalER,
        IExternalProvider externalAppointment)
    {
        _patientService = patientService;
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
        _externalER = externalER;
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
        RecordDTO dto = _externalER.FetchRecordByPatientId(externalId);
        await ProcessImportAsync(dto, patientId);
    }

    public async Task ImportFromAppointmentAsync(int patientId, int externalId)
    {
        RecordDTO dto = _externalAppointment.FetchRecordByPatientId(externalId);
        await ProcessImportAsync(dto, patientId);
    }

    private async Task ProcessImportAsync(RecordDTO dto, int patientId)
    {
        Patient patient = await _patientService.GetPatientDetailsAsync(patientId);

        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        MedicalRecord record = BuildRecordFromDTO(dto, patient.MedicalHistory.Id);
        int recordId = await _recordRepo.AddAsync(record);

        if (!string.IsNullOrWhiteSpace(dto.PrescribedMeds))
        {
            await CreatePrescriptionAsync(dto.PrescribedMeds, recordId);
        }
    }

    private Task CreatePrescriptionAsync(string medsString, int recordId)
    {
        string[] meds = medsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var prescription = new Prescription
        {
            RecordId = recordId,
            Date = DateTime.Now,
            DoctorNotes = "Imported from external provider",
            MedicationList = [.. meds.Select(m => new PrescriptionItem
            {
                MedName = m,
                Quantity = "1",
            })],
        };

        return _prescriptionRepo.AddAsync(prescription);
    }

    private static MedicalRecord BuildRecordFromDTO(RecordDTO dto, int historyId)
    {
        return new MedicalRecord
        {
            HistoryId = historyId,
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
