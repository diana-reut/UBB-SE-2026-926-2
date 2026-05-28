using System.Text;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.Patient;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PatientController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IBillingApiClient billingApiClient;

    public PatientController(IPatientApiClient patientApiClient, IBillingApiClient billingApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.billingApiClient = billingApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        int? selectedRecordId,
        CancellationToken cancellationToken)
    {
        try
        {
            PatientProfileViewModel model = await BuildProfileAsync(id, selectedRecordId, cancellationToken);
            ApplyTemporaryDiscount(model);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Prescription(
        int patientId,
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            Prescription? prescription = await patientApiClient.GetPrescriptionByRecordIdAsync(recordId, cancellationToken);
            if (prescription is null)
            {
                TempData["ErrorMessage"] = "This consultation does not have an associated prescription.";
                return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
            }

            PatientPrescriptionViewModel model = MapPrescription(prescription);
            ViewData["PatientId"] = patientId;
            ViewData["RecordId"] = recordId;
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyDiscount(
        int patientId,
        int recordId,
        int discount,
        CancellationToken cancellationToken)
    {
        if (discount is < 0 or > 100)
        {
            TempData["ErrorMessage"] = "Discount must be between 0 and 100.";
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }

        try
        {
            decimal basePrice = await billingApiClient.ComputeBasePriceAsync(patientId, recordId, cancellationToken);
            decimal discountedPrice = await billingApiClient.ApplyDiscountAsync(recordId, basePrice, discount, cancellationToken);

            TempData["SuccessMessage"] = $"Applied a {discount}% discount. Final price: {discountedPrice:C}.";
            TempData["TemporaryDiscount"] = discount.ToString();
            TempData["TemporaryDiscountedPrice"] = discountedPrice.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportRecord(
        int patientId,
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            RecordExportDataDto exportData = await patientApiClient.GetRecordExportDataAsync(recordId, cancellationToken);
            byte[] bytes = Encoding.UTF8.GetBytes(BuildExportText(exportData));
            string fileName = $"MedicalRecord_{exportData.Patient.FirstName}{exportData.Patient.LastName}_{exportData.Record.ConsultationDate:yyyyMMdd}.txt";
            return File(bytes, "text/plain", fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
    }

    private async Task<PatientProfileViewModel> BuildProfileAsync(
        int id,
        int? selectedRecordId,
        CancellationToken cancellationToken)
    {
        Patient patient = await patientApiClient.GetPatientDetailsAsync(id, cancellationToken);
        MedicalHistory? history = patient.MedicalHistory;

        List<PatientRecordViewModel> records = history?.MedicalRecords?
            .OrderByDescending(r => r.ConsultationDate)
            .Select(MapRecord)
            .ToList() ?? new List<PatientRecordViewModel>();

        PatientRecordViewModel? selectedRecord = selectedRecordId.HasValue
            ? records.FirstOrDefault(r => r.Id == selectedRecordId.Value)
            : records.FirstOrDefault();

        PatientPrescriptionViewModel? prescription = null;
        decimal? basePrice = null;
        decimal? finalPrice = null;

        if (selectedRecord is not null)
        {
            basePrice = await TryComputeBasePriceAsync(patient.Id, selectedRecord.Id, cancellationToken);
            finalPrice = selectedRecord.FinalPrice > 0 ? selectedRecord.FinalPrice : basePrice;
            prescription = await TryLoadPrescriptionAsync(selectedRecord.Id, cancellationToken);
        }

        return new PatientProfileViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.Dob,
            Sex = patient.Sex.ToString(),
            Cnp = patient.Cnp,
            PhoneNo = FormatPhoneNumber(patient.PhoneNo),
            EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
            IsArchived = patient.IsArchived,
            IsHighRisk = await TryLoadHighRiskAsync(patient.Id, cancellationToken),
            BloodType = history?.BloodType?.ToString() ?? "N/A",
            Rh = history?.Rh?.ToString() ?? "N/A",
            ChronicConditions = history?.ChronicConditions is { Count: > 0 }
                ? string.Join(", ", history.ChronicConditions)
                : "None",
            Allergies = await patientApiClient.GetPatientAllergiesAsync(patient.Id, cancellationToken),
            MedicalRecords = records,
            SelectedRecordId = selectedRecord?.Id,
            SelectedRecord = selectedRecord,
            SelectedPrescription = prescription,
            BasePrice = basePrice,
            FinalPrice = finalPrice
        };
    }

    private async Task<decimal?> TryComputeBasePriceAsync(
        int patientId,
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await billingApiClient.ComputeBasePriceAsync(patientId, recordId, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<PatientPrescriptionViewModel?> TryLoadPrescriptionAsync(
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            Prescription? prescription = await patientApiClient.GetPrescriptionByRecordIdAsync(recordId, cancellationToken);
            return prescription is null ? null : MapPrescription(prescription);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> TryLoadHighRiskAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            return await patientApiClient.IsHighRiskPatientAsync(patientId, cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private void ApplyTemporaryDiscount(PatientProfileViewModel model)
    {
        if (TempData["TemporaryDiscount"] is string discountText
            && int.TryParse(discountText, out int discount))
        {
            model.TemporaryDiscount = discount;
        }

        if (TempData["TemporaryDiscountedPrice"] is string priceText
            && decimal.TryParse(
                priceText,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal discountedPrice))
        {
            model.TemporaryDiscountedPrice = discountedPrice;
            model.FinalPrice = discountedPrice;
        }
    }

    private static PatientRecordViewModel MapRecord(MedicalRecord record)
    {
        return new PatientRecordViewModel
        {
            Id = record.Id,
            ConsultationDate = record.ConsultationDate,
            SourceType = record.SourceType.ToString(),
            StaffId = record.StaffId,
            Symptoms = record.Symptoms ?? "N/A",
            Diagnosis = record.Diagnosis ?? "N/A",
            BasePrice = record.BasePrice,
            FinalPrice = record.FinalPrice,
            DiscountApplied = record.DiscountApplied
        };
    }

    private static PatientPrescriptionViewModel MapPrescription(Prescription prescription)
    {
        return new PatientPrescriptionViewModel
        {
            Id = prescription.Id,
            Date = prescription.Date,
            DoctorNotes = prescription.DoctorNotes ?? "None",
            Items = prescription.MedicationList
                .Select(item => new PatientPrescriptionItemViewModel
                {
                    MedicationName = item.MedName,
                    Quantity = item.Quantity ?? string.Empty
                })
                .ToList()
        };
    }

    private static string BuildExportText(RecordExportDataDto data)
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine($"Patient: {data.Patient.FirstName} {data.Patient.LastName}");
        _ = builder.AppendLine($"CNP: {data.Patient.Cnp}");
        _ = builder.AppendLine($"Consultation Date: {data.Record.ConsultationDate:yyyy-MM-dd HH:mm}");
        _ = builder.AppendLine();
        _ = builder.AppendLine("Clinical Findings");
        _ = builder.AppendLine($"Symptoms: {data.Record.Symptoms ?? "N/A"}");
        _ = builder.AppendLine($"Diagnosis: {data.Record.Diagnosis ?? "N/A"}");
        _ = builder.AppendLine();
        _ = builder.AppendLine("Prescribed Treatment");

        if (data.Prescription is null || data.Items.Count == 0)
        {
            _ = builder.AppendLine("No prescription issued for this consultation.");
        }
        else
        {
            _ = builder.AppendLine($"Doctor Notes: {data.Prescription.DoctorNotes ?? "None"}");
            foreach (var item in data.Items)
            {
                _ = builder.AppendLine($"- {item.MedName}: {item.Quantity}");
            }
        }

        return builder.ToString();
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening patient details.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (normalized.StartsWith("+40", StringComparison.Ordinal))
        {
            normalized = $"0{normalized[3..]}";
        }

        if (!normalized.StartsWith('0') || normalized.Length != 10)
        {
            return phone;
        }

        return $"+40 {normalized.Substring(1, 3)} {normalized.Substring(4, 3)} {normalized.Substring(7, 3)}";
    }
}
