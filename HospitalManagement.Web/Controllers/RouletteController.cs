using Common.Data.Entity;
using HospitalManagement.Web.Models.Consultations;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class RouletteController : Controller
{
    // Set of discounts accepted from the client. The base wheel has segments
    // for 0/10/25/50/100; the "let it ride" double-or-nothing can also produce
    // 20 (10x2) or shove anything back to 0.
    private static readonly HashSet<int> AllowedSubmittedDiscounts = [0, 10, 20, 25, 50, 100];

    private readonly IPatientApiClient patientApiClient;
    private readonly IBillingApiClient billingApiClient;

    public RouletteController(IPatientApiClient patientApiClient, IBillingApiClient billingApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.billingApiClient = billingApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Spin(int patientId, int recordId)
    {
        Patient patient;
        try
        {
            patient = await patientApiClient.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Consultation", new { patientId, recordId });
        }

        MedicalRecord? record = patient.MedicalHistory?.MedicalRecords?
            .FirstOrDefault(r => r.Id == recordId);
        if (record is null)
        {
            TempData["ErrorMessage"] = "Consultation record not found.";
            return RedirectToAction("Details", "Admin", new { id = patientId });
        }

        decimal basePrice;
        try
        {
            basePrice = await billingApiClient.ComputeBasePriceAsync(patientId, recordId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException)
        {
            basePrice = record.BasePrice;
        }

        var model = new DiscountRouletteViewModel
        {
            PatientId = patientId,
            RecordId = recordId,
            PatientFullName = $"{patient.FirstName} {patient.LastName}",
            BasePrice = basePrice
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Spin(int patientId, int recordId, decimal basePrice, int discount)
    {
        // The client picks the discount (so the wheel actually lands on the
        // correct color) — we just validate it's one of the allowed values
        // and persist the result.
        if (!AllowedSubmittedDiscounts.Contains(discount))
        {
            TempData["ErrorMessage"] = "Invalid discount value.";
            return RedirectToAction("Details", "Consultation", new { patientId, recordId });
        }

        decimal finalPrice;
        try
        {
            finalPrice = await billingApiClient.ApplyDiscountAsync(basePrice, discount, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException)
        {
            // Fallback to local calculation if the billing API is unreachable.
            finalPrice = basePrice * (1 - discount / 100m);
        }

        var result = new DiscountAppliedResult { Discount = discount, FinalPrice = finalPrice };
        HttpContext.Session.SetString(SessionKeyForRecord(recordId), JsonSerializer.Serialize(result));

        TempData["SuccessMessage"] = discount == 0
            ? "Unlucky — no discount this time."
            : $"You won a {discount}% discount!";

        return RedirectToAction("Details", "Consultation", new { patientId, recordId });
    }

    internal static string SessionKeyForRecord(int recordId) => $"discount:{recordId}";
}
