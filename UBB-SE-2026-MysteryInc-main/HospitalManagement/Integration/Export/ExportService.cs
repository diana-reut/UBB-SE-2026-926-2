using System.Collections.Generic;
using Common.Data.Entity;
using Common.Data.Repository;
using Common.Data.Entity;

namespace HospitalManagement.Integration.Export;

internal class ExportService : IExportService
{
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository _prescriptionRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IMedicalHistoryRepository _historyRepo;

    public ExportService(
        IMedicalRecordRepository recordRepo,
        IPrescriptionRepository prescriptionRepo,
        IPatientRepository patientRepo,
        IMedicalHistoryRepository historyRepo)
    {
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
        _patientRepo = patientRepo;
        _historyRepo = historyRepo;
    }

    public async Task<string> ExportRecordToPDFAsync(int recordId)
    {
        MedicalRecord record = await _recordRepo.GetByIdAsync(recordId)
            ?? throw new ExportException($"MedicalRecord with ID={recordId} not found.");

        MedicalHistory history = await _historyRepo.GetByIdAsync(record.HistoryId)
            ?? throw new ExportException($"MedicalHistory for record ID={recordId} not found.");

        Patient patient = await _patientRepo.GetByIdAsync(history.PatientId)
            ?? throw new ExportException($"Patient for history ID={history.Id} not found.");

        var items = new List<PrescriptionItem>();
        Prescription? prescription = await _prescriptionRepo.GetByRecordIdAsync(recordId);
        if (prescription is not null)
        {
            items = await _prescriptionRepo.GetItemsAsync(prescription.Id);
        }

        return PDFGenerator.GenerateRecordPDF(record, patient, prescription, items);
    }
}
