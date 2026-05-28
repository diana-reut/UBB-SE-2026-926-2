using Common.Data.Entity;
using HospitalManagement.Web.Models.BloodCompatibility;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

// GET: /BloodCompatibility/Donors?patientId=5
[Authorize]
public class BloodCompatibilityController : Controller
{
    private readonly IBloodCompatibilityApiClient bloodCompatibilityApiClient;
    private readonly IPatientApiClient patientApiClient;

    public BloodCompatibilityController(
        IBloodCompatibilityApiClient bloodCompatibilityApiClient,
        IPatientApiClient patientApiClient)
    {
        this.bloodCompatibilityApiClient = bloodCompatibilityApiClient;
        this.patientApiClient = patientApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Donors(int patientId)
    {
        Patient? patient;
        try
        {
            patient = await patientApiClient.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }

        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction("Index", "Admin");
        }

        var model = new BloodDonorsViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
        };

        // Guard: patient must have blood type & Rh on record
        if (patient.MedicalHistory?.BloodType is null || patient.MedicalHistory?.Rh is null)
        {
            model.StatusMessage =
                "The selected patient needs a blood type and Rh factor in their medical history first.";
            return View(model);
        }

        List<Patient> topDonors;
        try
        {
            topDonors = await bloodCompatibilityApiClient
                .GetTopCompatibleDonorsAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            model.StatusMessage = ex.Message;
            return View(model);
        }

        model.Donors = topDonors.Select(donor => new DonorMatchViewModel
        {
            FirstName = donor.FirstName,
            LastName = donor.LastName,
            Cnp = donor.Cnp,
            BloodType = donor.MedicalHistory?.BloodType?.ToString() ?? "Unknown",
            RhFactor = donor.MedicalHistory?.Rh?.ToString() ?? "Unknown",
            Score = CalculateScore(donor, patient),
        }).ToList();

        if (model.Donors.Count == 0)
        {
            model.StatusMessage = "No compatible blood donors were found for this patient.";
        }

        return View(model);
    }

    private static int CalculateScore(Patient donor, Patient recipient)
    {
        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
        {
            return 0;
        }

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
                    && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? 50
            : 25;

        int ageGap = Math.Abs(donor.Dob.Year - recipient.Dob.Year);
        total += Math.Max(0, 30 - (ageGap / 5 * 5));
        total += donor.Sex == recipient.Sex ? 20 : 10;

        return total;
    }
}