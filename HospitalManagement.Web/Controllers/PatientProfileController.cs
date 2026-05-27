using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MigratedPatientProfileModel = HospitalManagement.Web.Models.PatientProfile.PatientProfileModel;
using MigratedPatientProfileViewModel = HospitalManagement.Web.Models.PatientProfile.PatientProfileViewModel;
using PatientsMedicalRecordViewModel = HospitalManagement.Web.Models.Patients.MedicalRecordViewModel;
using PatientsPatientProfileViewModel = HospitalManagement.Web.Models.Patients.PatientProfileViewModel;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PatientProfileController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IBillingApiClient billingApiClient;
    private readonly IErWorkflowApiClient erWorkflowApiClient;
    private readonly IAppointmentImportProvider appointmentImportProvider;

    public PatientProfileController(
        IPatientApiClient patientApiClient,
        IBillingApiClient billingApiClient,
        IErWorkflowApiClient erWorkflowApiClient,
        IAppointmentImportProvider appointmentImportProvider)
    {
        this.patientApiClient = patientApiClient;
        this.billingApiClient = billingApiClient;
        this.erWorkflowApiClient = erWorkflowApiClient;
        this.appointmentImportProvider = appointmentImportProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, int? selectedRecordId, CancellationToken cancellationToken = default)
    {
        try
        {
            Patient patient = await patientApiClient.GetPatientDetailsAsync(id, cancellationToken);
            List<string> allergies = await patientApiClient.GetPatientAllergiesAsync(id, cancellationToken);
            bool isHighRisk = await patientApiClient.IsHighRiskAsync(id, cancellationToken);

            var model = new PatientsPatientProfileViewModel
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
                    .Select(r => new PatientsMedicalRecordViewModel
                    {
                        Id = r.Id,
                        ConsultationDate = r.ConsultationDate,
                        SourceType = r.SourceType.ToString(),
                        StaffId = r.StaffId,
                        Symptoms = r.Symptoms ?? "N/A",
                        Diagnosis = r.Diagnosis ?? "N/A",
                    }).ToList() ?? new List<PatientsMedicalRecordViewModel>(),
                SelectedRecordId = selectedRecordId,
            };

            foreach (PatientsMedicalRecordViewModel record in model.MedicalRecords)
            {
                try
                {
                    Prescription? prescription = await patientApiClient
                        .GetPrescriptionByRecordIdAsync(record.Id, cancellationToken);
                    record.PrescriptionId = prescription?.Id;
                }
                catch
                {
                    record.PrescriptionId = null;
                }
            }

            ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            return View("~/Views/Patients/PatientProfile.cshtml", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load patient profile: " + ex.Message;
            return RedirectToAction("Dashboard", "MedicalStaff");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        MigratedPatientProfileModel model;
        try
        {
            model = await BuildProfileModelAsync(id, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }

        MigratedPatientProfileViewModel vm = MigratedPatientProfileViewModel.FromModel(model);
        if (TempData["SuccessMessage"] is string success)
        {
            vm.SuccessMessage = success;
        }

        if (TempData["ErrorMessage"] is string error)
        {
            vm.ErrorMessage = error;
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> SelectRecord(int patientId, int recordId)
    {
        MigratedPatientProfileModel model;
        try
        {
            model = await BuildProfileModelAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        MedicalRecord? record = model.MedicalRecords.FirstOrDefault(r => r.Id == recordId);
        if (record is not null)
        {
            model.SelectedRecordId = recordId;
            try
            {
                model.BasePrice = await billingApiClient.ComputeBasePriceAsync(
                    patientId,
                    recordId,
                    HttpContext.RequestAborted);
                model.FinalPrice = record.FinalPrice > 0 ? record.FinalPrice : model.BasePrice;
                model.DiscountApplied = record.DiscountApplied;
            }
            catch
            {
                model.BasePrice = record.BasePrice;
                model.FinalPrice = record.FinalPrice;
                model.DiscountApplied = record.DiscountApplied;
            }
        }

        return View("Details", MigratedPatientProfileViewModel.FromModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> ViewPrescription(int patientId, int recordId)
    {
        MigratedPatientProfileModel model;
        try
        {
            model = await BuildProfileModelAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        model.SelectedRecordId = recordId;
        try
        {
            Prescription? prescription = await patientApiClient
                .GetPrescriptionByRecordIdAsync(recordId, HttpContext.RequestAborted);
            if (prescription is null)
            {
                TempData["ErrorMessage"] = "This consultation does not have an associated prescription.";
                return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
            }

            model.SelectedPrescription = prescription;
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Could not load prescription: {ex.Message}";
            return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = $"Could not load prescription: {ex.Message}";
            return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
        }

        return View("Details", MigratedPatientProfileViewModel.FromModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> ExportRecord(int recordId, CancellationToken cancellationToken = default)
    {
        try
        {
            RecordExportDataDto exportData = await patientApiClient.GetRecordExportDataAsync(recordId, cancellationToken);

            byte[] pdfBytes;
            using (var stream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var doc = new iText.Layout.Document(pdf);

                doc.Add(new iText.Layout.Element.Paragraph(
                    $"Patient: {exportData.Patient.FirstName} {exportData.Patient.LastName}").SetFontSize(16));
                doc.Add(new iText.Layout.Element.Paragraph($"CNP: {exportData.Patient.Cnp}"));
                doc.Add(new iText.Layout.Element.Paragraph(
                    $"Consultation Date: {exportData.Record.ConsultationDate:dd-MM-yyyy HH:mm}"));
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
                    doc.Add(new iText.Layout.Element.Paragraph(
                        $"Doctor Notes: {exportData.Prescription.DoctorNotes ?? "None"}"));
                    doc.Add(new iText.Layout.Element.Paragraph("Medications:"));
                    foreach (var item in exportData.Items)
                    {
                        doc.Add(new iText.Layout.Element.Paragraph($"  - {item.MedName}: {item.Quantity}"));
                    }
                }

                doc.Close();
                pdfBytes = stream.ToArray();
            }

            string fileName =
                $"MedicalRecord_{exportData.Patient.FirstName}{exportData.Patient.LastName}_{exportData.Record.ConsultationDate:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Export failed: " + ex.Message;
            return RedirectToAction(nameof(Index), new { id = 0 });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyDiscount(int patientId, int recordId, int discountPercent)
    {
        if (discountPercent is < 0 or > 100)
        {
            TempData["ErrorMessage"] = "Discount must be between 0 and 100%.";
            return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
        }

        try
        {
            decimal basePrice = await billingApiClient.ComputeBasePriceAsync(
                patientId,
                recordId,
                HttpContext.RequestAborted);
            decimal finalPrice = await billingApiClient.ApplyDiscountAsync(
                recordId,
                basePrice,
                discountPercent,
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = $"Discount of {discountPercent}% applied. Final price: {finalPrice:C}.";
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Could not apply discount: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = $"Could not apply discount: {ex.Message}";
        }

        return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportRecord(int patientId, int recordId)
    {
        try
        {
            var data = await patientApiClient.GetRecordExportDataAsync(recordId, HttpContext.RequestAborted);
            TempData["SuccessMessage"] =
                $"Record #{recordId} exported. Patient: {data.Patient?.FullName}, Date: {data.Record?.ConsultationDate:yyyy-MM-dd}.";
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
        }

        return RedirectToAction(nameof(SelectRecord), new { patientId, recordId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromEr(int patientId)
    {
        try
        {
            Patient patient = await patientApiClient.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
            if (patient.MedicalHistory is null)
            {
                throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
            }

            var existingErSourceIds = patient.MedicalHistory.MedicalRecords?
                .Where(record => record.SourceType == Common.Data.Entity.Enums.SourceType.ER)
                .Select(record => record.SourceId)
                .ToHashSet() ?? new HashSet<int>();

            Examination? candidateExam = (await erWorkflowApiClient.GetPatientExaminationHistoryAsync(
                    patient.Cnp,
                    HttpContext.RequestAborted))
                .OrderByDescending(examination => examination.Exam_Time)
                .FirstOrDefault(examination => !existingErSourceIds.Contains(examination.Visit_ID));

            if (candidateExam is null)
            {
                throw new InvalidOperationException("No new ER examination is available to import for this patient.");
            }

            ERExaminationSummaryDto? summary = await erWorkflowApiClient.GetExaminationSummaryAsync(
                candidateExam.Visit_ID,
                HttpContext.RequestAborted);
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

            await ProcessImportAsync(dto, patient, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "ER records imported correctly.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { id = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromStaff(int patientId)
    {
        try
        {
            Patient patient = await patientApiClient.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
            RecordDTO dto = appointmentImportProvider.FetchRecordByPatientId(patientId);
            await ProcessImportAsync(dto, patient, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Staff records imported correctly.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { id = patientId });
    }

    private async Task<MigratedPatientProfileModel> BuildProfileModelAsync(
        int patientId,
        CancellationToken cancellationToken)
    {
        Patient patient = await patientApiClient.GetPatientDetailsAsync(patientId, cancellationToken);
        patient.MedicalHistory ??= new MedicalHistory();
        patient.MedicalHistory.MedicalRecords ??= new List<MedicalRecord>();

        List<MedicalRecord> records = patient.MedicalHistory.Id > 0
            ? await patientApiClient.GetMedicalRecordsAsync(patient.MedicalHistory.Id, cancellationToken)
            : new List<MedicalRecord>();
        List<string> allergies = await patientApiClient.GetPatientAllergiesAsync(patientId, cancellationToken);
        var history = patient.MedicalHistory;

        return new MigratedPatientProfileModel
        {
            PatientId = patientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.Dob,
            BloodType = history.BloodType?.ToString(),
            Rh = history.Rh?.ToString(),
            ChronicConditionsFormatted = history.ChronicConditions is { Count: > 0 }
                ? string.Join(", ", history.ChronicConditions)
                : "None",
            Allergies = allergies,
            MedicalRecords = records.OrderByDescending(r => r.ConsultationDate).ToList(),
            CachedAt = DateTime.UtcNow,
        };
    }

    private async Task ProcessImportAsync(RecordDTO dto, Patient patient, CancellationToken cancellationToken)
    {
        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        int recordId = await patientApiClient.CreateMedicalRecordAsync(
            patient.Id,
            BuildRecordFromDto(dto),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.PrescribedMeds))
        {
            await CreatePrescriptionAsync(dto.PrescribedMeds, recordId, cancellationToken);
        }
    }

    private async Task CreatePrescriptionAsync(string medsString, int recordId, CancellationToken cancellationToken)
    {
        string[] meds = medsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var prescription = new CreatePrescriptionDto
        {
            Date = DateTime.Now,
            DoctorNotes = "Imported from external provider",
            Items = meds.Select(medication => new CreatePrescriptionItemDto
            {
                MedName = medication,
                Quantity = "1",
            }).ToList(),
        };

        await patientApiClient.CreatePrescriptionForRecordAsync(recordId, prescription, cancellationToken);
    }

    private static CreateMedicalRecordDto BuildRecordFromDto(RecordDTO dto)
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
