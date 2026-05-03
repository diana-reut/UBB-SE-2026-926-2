using System.Collections.Generic;
using HospitalManagement.Entity;
using HospitalManagement.Repository;

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

    public string ExportRecordToPDF(int recordId)
    {
        MedicalRecord? record = _recordRepo.GetById(recordId) ?? throw new ExportException($"MedicalRecord with ID={recordId} not found.");

        MedicalHistory? history = _historyRepo.GetByPatientId(record.HistoryId) ?? throw new ExportException($"MedicalHistory for record ID={recordId} not found.");

        Patient? patient = _patientRepo.GetById(history.PatientId) ?? throw new ExportException($"Patient for history ID={history.Id} not found.");

        // THE FIX: Use the PrescriptionRepo to find the prescription!
        var items = new List<PrescriptionItem>();
        Prescription? prescription = _prescriptionRepo.GetByRecordId(recordId);

        if (prescription is not null)
        {
            items = _prescriptionRepo.GetItems(prescription.Id);
        }

        return PDFGenerator.GenerateRecordPDF(record, patient, prescription, items);
    }
}
