using Common.Data.Entity;
using Common.Data.Integration;
using HospitalManagement.Web.Models.Pharmacist;
using HospitalManagement.Web.Models.Prescription;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PharmacistController : Controller
{
    private const int PageSize = 9;

    private readonly IPrescriptionApiClient prescriptionApiClient;
    private readonly IAddictDetectionApiClient addictDetectionApiClient;

    public PharmacistController(
        IPrescriptionApiClient prescriptionApiClient,
        IAddictDetectionApiClient addictDetectionApiClient)
    {
        this.prescriptionApiClient = prescriptionApiClient;
        this.addictDetectionApiClient = addictDetectionApiClient;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction("Feed", "Prescription");

    // Legacy route — the table view was replaced by the card-grid Feed.
    // Redirect any existing links/bookmarks to the new view.
    [HttpGet]
    public IActionResult Prescriptions() => RedirectToAction("Feed", "Prescription");

    [HttpGet]
    public async Task<IActionResult> PrescriptionDetail(int id, int? returnPatientId = null, int? returnRecordId = null)
    {
        Prescription? prescription;
        try
        {
            prescription = await prescriptionApiClient.GetPrescriptionDetailsAsync(id, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Prescriptions));
        }

        if (prescription is null)
        {
            TempData["ErrorMessage"] = $"Prescription #{id} not found.";
            return RedirectToAction(nameof(Prescriptions));
        }

        var model = new PrescriptionDetailViewModel
        {
            Id = prescription.Id,
            PatientName = prescription.PatientName,
            DoctorName = prescription.DoctorName,
            Date = prescription.Date,
            DoctorNotes = prescription.DoctorNotes ?? "No notes provided",
            ReturnPatientId = returnPatientId,
            ReturnRecordId = returnRecordId,
            Medications = (prescription.MedicationList ?? new List<PrescriptionItem>())
                .Select(m => new PrescriptionItemViewModel
                {
                    MedName = m.MedName,
                    Quantity = m.Quantity
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Addicts()
    {
        List<Patient> candidates;
        try
        {
            candidates = await addictDetectionApiClient.GetCandidatesAsync(HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            candidates = new List<Patient>();
        }

        var model = new AddictListViewModel
        {
            // Match WinUI behavior: once reported, the candidate disappears
            // from the list entirely (no "Reported to Police" badge row).
            Candidates = candidates
                .Where(p => !p.IsPoliceNotified)
                .Select(p => new AddictCandidateViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    IsPoliceNotified = p.IsPoliceNotified
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuildPoliceReport(int patientId)
    {
        try
        {
            string report = await addictDetectionApiClient.BuildPoliceReportAsync(patientId, HttpContext.RequestAborted);
            TempData["PoliceReportText"] = report;
            TempData["PoliceReportPatientId"] = patientId;
            return RedirectToAction(nameof(PoliceAlert));
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Addicts));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Addicts));
        }
    }

    [HttpGet]
    public IActionResult PoliceAlert()
    {
        string? reportText = TempData["PoliceReportText"] as string;
        if (string.IsNullOrEmpty(reportText))
        {
            return RedirectToAction(nameof(Addicts));
        }

        int patientId = TempData["PoliceReportPatientId"] as int? ?? 0;
        bool alreadySent = TempData["PoliceReportSent"] as bool? ?? false;

        var model = new PoliceAlertViewModel
        {
            ReportText = reportText,
            PatientId = patientId,
            AlreadySent = alreadySent
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPoliceReport(int patientId, string reportText)
    {
        try
        {
            await addictDetectionApiClient.MarkPoliceNotifiedAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["PoliceReportText"] = reportText;
            TempData["PoliceReportPatientId"] = patientId;
            return RedirectToAction(nameof(PoliceAlert));
        }

        TempData["PoliceReportText"] = reportText;
        TempData["PoliceReportPatientId"] = patientId;
        TempData["PoliceReportSent"] = true;
        return RedirectToAction(nameof(PoliceAlert));
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? TryParseNullableInt(string? value) =>
        int.TryParse(value, out int result) ? result : null;
}
