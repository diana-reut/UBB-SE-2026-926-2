using Common.Data.Entity;
using HospitalManagement.Web.Models.PatientProfile;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PatientProfileController : Controller
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IPatientApiClient patientApiClient;
    private readonly IBillingApiClient billingApiClient;
    private readonly IMemoryCache cache;

    public PatientProfileController(
        IPatientApiClient patientApiClient,
        IBillingApiClient billingApiClient,
        IMemoryCache cache)
    {
        this.patientApiClient = patientApiClient;
        this.billingApiClient = billingApiClient;
        this.cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        PatientProfileModel model;
        try
        {
            model = await GetOrBuildProfileAsync(id, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }

        PatientProfileViewModel vm = PatientProfileViewModel.FromModel(model);
        if (TempData["SuccessMessage"] is string success) vm.SuccessMessage = success;
        if (TempData["ErrorMessage"] is string error) vm.ErrorMessage = error;
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> SelectRecord(int patientId, int recordId)
    {
        PatientProfileModel model;
        try
        {
            model = await GetOrBuildProfileAsync(patientId, HttpContext.RequestAborted);
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
                    patientId, recordId, HttpContext.RequestAborted);
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

        return View("Details", PatientProfileViewModel.FromModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> ViewPrescription(int patientId, int recordId)
    {
        PatientProfileModel model;
        try
        {
            model = await GetOrBuildProfileAsync(patientId, HttpContext.RequestAborted);
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

        return View("Details", PatientProfileViewModel.FromModel(model));
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
                patientId, recordId, HttpContext.RequestAborted);
            decimal finalPrice = await billingApiClient.ApplyDiscountAsync(
                basePrice, discountPercent, HttpContext.RequestAborted);

            cache.Remove(CacheKey(patientId));

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

    private static string CacheKey(int patientId) => $"patient_profile_{patientId}";
    private async Task<PatientProfileModel> GetOrBuildProfileAsync(
        int patientId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey(patientId), out PatientProfileModel? cached) && cached is not null)
        {
            return cached;
        }

        PatientProfileModel model = await BuildProfileModelAsync(patientId, cancellationToken);
        cache.Set(CacheKey(patientId), model, CacheDuration);
        return model;
    }

    private async Task<PatientProfileModel> BuildProfileModelAsync(
        int patientId, CancellationToken cancellationToken)
    {
        Patient patient = await patientApiClient.GetPatientDetailsAsync(patientId, cancellationToken);
        patient.MedicalHistory ??= new MedicalHistory();
        patient.MedicalHistory.MedicalRecords ??= [];

        List<MedicalRecord> records = patient.MedicalHistory.Id > 0
            ? await patientApiClient.GetMedicalRecordsAsync(patient.MedicalHistory.Id, cancellationToken)
            : [];
        List<string> allergies = await patientApiClient.GetPatientAllergiesAsync(patientId, cancellationToken);
        var history = patient.MedicalHistory;

        return new PatientProfileModel
        {
            PatientId = patientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.Dob,
            BloodType = history.BloodType?.ToString(),
            Rh = history.Rh?.ToString(),
            ChronicConditionsFormatted = history.ChronicConditions is { Count: > 0 }
                ? string.Join(", ", history.ChronicConditions) : "None",
            Allergies = allergies,
            MedicalRecords = records.OrderByDescending(r => r.ConsultationDate).ToList(),
            CachedAt = DateTime.UtcNow,
        };
    }
}
