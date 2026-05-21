using Common.Data.Entity;
using HospitalManagement.Web.Models.Consultations;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class ConsultationController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IBillingApiClient billingApiClient;

    public ConsultationController(IPatientApiClient patientApiClient, IBillingApiClient billingApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.billingApiClient = billingApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int patientId, int recordId)
    {
        Patient patient;
        try
        {
            patient = await patientApiClient.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
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

        int? discountApplied = record.DiscountApplied;
        decimal finalPrice = record.DiscountApplied.HasValue ? record.FinalPrice : basePrice;

        // Look up the prescription tied to this consultation record (if any),
        // so the "View Prescription" button can link directly to it.
        int? prescriptionId = null;
        try
        {
            Prescription? prescription = await patientApiClient.GetPrescriptionByRecordIdAsync(recordId, HttpContext.RequestAborted);
            prescriptionId = prescription?.Id;
        }
        catch (InvalidOperationException)
        {
            // Non-fatal: the rest of the consultation page still works without the link.
            prescriptionId = null;
        }

        var model = new ConsultationDetailsViewModel
        {
            RecordId = record.Id,
            PatientId = patient.Id,
            PatientFirstName = patient.FirstName,
            PatientLastName = patient.LastName,
            SourceType = record.SourceType.ToString(),
            StaffId = record.StaffId,
            ConsultationDate = record.ConsultationDate,
            Symptoms = record.Symptoms ?? "N/A",
            Diagnosis = record.Diagnosis ?? "N/A",
            BasePrice = basePrice,
            FinalPrice = finalPrice,
            DiscountApplied = discountApplied,
            PrescriptionId = prescriptionId,
            IsArchived = patient.IsArchived
        };

        return View(model);
    }
}
