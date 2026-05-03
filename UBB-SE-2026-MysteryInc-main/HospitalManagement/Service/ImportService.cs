using System;
using System.Linq;
using HospitalManagement.Entity;
using HospitalManagement.Entity.DTOs;
using HospitalManagement.Repository;
using HospitalManagement.Integration.External;

namespace HospitalManagement.Service;

internal class ImportService : IImportService
{
    private readonly IPatientService _patientService;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository _prescriptionRepo;
    private readonly IExternalProvider _externalER;
    private readonly IExternalProvider _externalAppointment;

    public ImportService(
        IPatientService patientService,
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

    // SV10: Import from ER. PatientId = id , ExternalId = CNP
    public void ImportFromER(int patientId, int externalId)
    {
        RecordDTO dto = _externalER.FetchRecordByPatientId(externalId);
        ProcessImport(dto, patientId);
    }

    public void ImportFromAppointment(int patientId, int externalId)
    {
        RecordDTO dto = _externalAppointment.FetchRecordByPatientId(externalId);
        ProcessImport(dto, patientId);
    }

    // main flow
    private void ProcessImport(RecordDTO dto, int patientId)
    {
        Patient patient = _patientService.GetPatientDetails(patientId);

        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        MedicalRecord record = BuildRecordFromDTO(dto, patient.MedicalHistory.Id);

        // save Record and get the new ID
        int recordId = _recordRepo.Add(record);

        // mape and save prescription
        if (!string.IsNullOrWhiteSpace(dto.PrescribedMeds))
        {
            CreatePrescription(dto.PrescribedMeds, recordId);
        }
    }

    // create linked prescription
    // SV11 - Prescription with prescription items
    private void CreatePrescription(string medsString, int recordId)
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

        _prescriptionRepo.Add(prescription);
    }

    // Built Record from DTO logic
    private static MedicalRecord BuildRecordFromDTO(RecordDTO dto, int historyId)
    {
        return new MedicalRecord
        {
            HistoryId = historyId,
            SourceType = dto.SourceType,
            SourceId = dto.ExternalRecordId,
            StaffId = 1, // TODO : next team, replace this with staff id when you have login/Sign up logic and id actually makes sense
            Symptoms = dto.Symptoms,
            Diagnosis = dto.TemporaryDiagnosis,
            ConsultationDate = dto.ConsultationDate,
            BasePrice = 0,
            FinalPrice = 0,
            PoliceNotified = false,
        };
    }
}
