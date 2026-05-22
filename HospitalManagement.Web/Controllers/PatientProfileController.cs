using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.MedicalStaff;
using HospitalManagement.Web.Models.Patients;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PatientProfileController : Controller
{
    private readonly IPatientApiClient _patientApiClient;

    public PatientProfileController(IPatientApiClient patientApiClient)
    {
        _patientApiClient = patientApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, int? selectedRecordId, CancellationToken cancellationToken = default) { 
        try
        {
            Patient patient = await _patientApiClient.GetPatientDetailsAsync(id, cancellationToken);
            List<string> allergies = await _patientApiClient.GetPatientAllergiesAsync(id, cancellationToken);
            bool isHighRisk = await _patientApiClient.IsHighRiskAsync(id, cancellationToken);

            var model = new PatientProfileViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Cnp = patient.Cnp,
                BloodType = patient.MedicalHistory?.BloodType?.ToString() ?? "N/A",
                Rh = patient.MedicalHistory?.Rh?.ToString() ?? "N/A",
                FormattedAllergies = allergies.Count > 0 ? string.Join(", ", allergies) : "None",
                FormattedChronicConditions = patient.MedicalHistory?.ChronicConditions is { Count: > 0 }
                    ? string.Join(", ", patient.MedicalHistory.ChronicConditions)
                    : "None",
                IsHighRisk = isHighRisk,
                MedicalRecords = patient.MedicalHistory?.MedicalRecords?
                    .OrderByDescending(r => r.ConsultationDate)
                    .Select(r => new MedicalRecordViewModel
                    {
                        Id = r.Id,
                        ConsultationDate = r.ConsultationDate,
                        SourceType = r.SourceType.ToString(),
                        StaffId = r.StaffId,
                        Symptoms = r.Symptoms ?? "N/A",
                        Diagnosis = r.Diagnosis ?? "N/A"
                    }).ToList() ?? []
            };

            foreach (var record in model.MedicalRecords)
            {
                try
                {
                    Prescription? prescription = await _patientApiClient.GetPrescriptionByRecordIdAsync(record.Id, cancellationToken);
                    record.PrescriptionId = prescription?.Id;
                }
                catch
                {
                    record.PrescriptionId = null;
                }
            }

            model.SelectedRecordId = selectedRecordId;
            return View("~/Views/Patients/PatientProfile.cshtml", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load patient profile: " + ex.Message;
            return RedirectToAction("Dashboard", "MedicalStaff");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportRecord(int recordId, CancellationToken cancellationToken = default)
    {
        try
        {
            RecordExportDataDto exportData = await _patientApiClient.GetRecordExportDataAsync(recordId, cancellationToken);

            byte[] pdfBytes;
            using (var stream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var doc = new iText.Layout.Document(pdf);

                doc.Add(new iText.Layout.Element.Paragraph($"Patient: {exportData.Patient.FirstName} {exportData.Patient.LastName}").SetFontSize(16));
                doc.Add(new iText.Layout.Element.Paragraph($"CNP: {exportData.Patient.Cnp}"));
                doc.Add(new iText.Layout.Element.Paragraph($"Consultation Date: {exportData.Record.ConsultationDate:dd-MM-yyyy HH:mm}"));
                doc.Add(new iText.Layout.Element.Paragraph("\n"));
                doc.Add(new iText.Layout.Element.Paragraph("Section 1: Clinical Findings").SetFontSize(14));
                doc.Add(new iText.Layout.Element.Paragraph($"Symptoms: {exportData.Record.Symptoms ?? "N/A"}"));
                doc.Add(new iText.Layout.Element.Paragraph($"Diagnosis: {exportData.Record.Diagnosis ?? "N/A"}"));
                doc.Add(new iText.Layout.Element.Paragraph("\n"));
                doc.Add(new iText.Layout.Element.Paragraph("Section 2: Prescribed Treatment").SetFontSize(14));

                if (exportData.Prescription is null || exportData.Items.Count == 0)
                {
                    doc.Add(new iText.Layout.Element.Paragraph("No prescription issued for this consultation."));
                }
                else
                {
                    doc.Add(new iText.Layout.Element.Paragraph($"Doctor Notes: {exportData.Prescription.DoctorNotes ?? "None"}"));
                    doc.Add(new iText.Layout.Element.Paragraph("Medications:"));
                    foreach (var item in exportData.Items)
                        doc.Add(new iText.Layout.Element.Paragraph($"  - {item.MedName}: {item.Quantity}"));
                }

                doc.Close();
                pdfBytes = stream.ToArray();
            }

            string fileName = $"MedicalRecord_{exportData.Patient.FirstName}{exportData.Patient.LastName}_{exportData.Record.ConsultationDate:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Export failed: " + ex.Message;
            return RedirectToAction(nameof(Index), new { id = 0 });
        }
    }
}