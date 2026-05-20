using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.MedicalStaff;
using HospitalManagement.Web.Models.Patients;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class MedicalStaffController : Controller
{
    private readonly IPatientApiClient _patientApiClient;
    private readonly IPrescriptionApiClient _prescriptionApiClient;

    public MedicalStaffController(IPatientApiClient patientApiClient, IPrescriptionApiClient prescriptionApiClient)
    {
        _patientApiClient = patientApiClient;
        _prescriptionApiClient = prescriptionApiClient;
    }
    [HttpGet]
    public async Task<IActionResult> Dashboard(
        string? searchQuery,
        int? selectedId,
        CancellationToken cancellationToken = default)
    {
        var model = new MedicalStaffDashboardViewModel
        {
            SearchQuery = searchQuery,
            HasSearched = searchQuery is not null,
            SelectedPatientId = selectedId
        };

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            try
            {
                string trimmed = searchQuery.Trim();
                var dto = trimmed.Length == 13 && trimmed.All(char.IsDigit)
                    ? new SearchPatientsDto { Cnp = trimmed }
                    : new SearchPatientsDto { NamePart = trimmed };

                List<Patient> results = await _patientApiClient.SearchPatientsAsync(dto, cancellationToken);

                if (results.Count == 0)
                    model.ErrorMessage = "There are no patients with this name or CNP.";
                else
                    model.SearchResults = results.Select(p => new PatientSearchResultViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Cnp = p.Cnp,
                        Dob = p.Dob
                    }).ToList();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Database connection error: " + ex.Message;
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PatientProfile(int id, int? selectedRecordId, CancellationToken cancellationToken = default)
    {
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
                        Diagnosis = r.Diagnosis ?? "N/A",
                        PrescriptionId = r.Prescription?.Id
                    }).ToList() ?? []
            };
            model.SelectedRecordId = selectedRecordId;
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
            return RedirectToAction(nameof(Dashboard));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportRecord(int recordId, CancellationToken cancellationToken = default)
    {
        try
        {
            RecordExportDataDto exportData = await _patientApiClient.GetRecordExportDataAsync(recordId, cancellationToken);

            if (exportData == null)
                throw new Exception("exportData is null");

            if (exportData.Patient == null)
                throw new Exception("Patient is null");

            if (exportData.Record == null)
                throw new Exception("Record is null");

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
            return Content(ex.ToString());
        }
    }

    [HttpGet]
    public async Task<IActionResult> PrescriptionDetails(int prescriptionId, int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            Prescription prescription = await _prescriptionApiClient.GetPrescriptionDetailsAsync(prescriptionId, cancellationToken);

            var model = new PrescriptionDetailsViewModel
            {
                Id = prescription.Id,
                PatientName = prescription.PatientName ?? string.Empty,
                DoctorName = prescription.DoctorName ?? string.Empty,
                DoctorNotes = prescription.DoctorNotes ?? string.Empty,
                Date = prescription.Date,
                ReturnPatientId = patientId,
                Items = prescription.MedicationList?.Select(i => new PrescriptionItemViewModel
                {
                    MedName = i.MedName,
                    Quantity = i.Quantity
                }).ToList() ?? []
            };

            return View("~/Views/Patients/PrescriptionDetails.cshtml", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load prescription: " + ex.Message;
            return RedirectToAction(nameof(PatientProfile), new { id = patientId });
        }
    }
}