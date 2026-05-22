using Common.Data.Entity;
using HospitalManagement.Web.Models.Transplant;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class OrganDonorController : Controller
{
    private readonly ITransplantApiClient transplantApiClient;
    private readonly IPatientApiClient patientApiClient;

    public OrganDonorController(
        ITransplantApiClient transplantApiClient,
        IPatientApiClient patientApiClient)
    {
        this.transplantApiClient = transplantApiClient;
        this.patientApiClient = patientApiClient;
    }

    // GET: /OrganDonor/Assign?patientId=5
    // GET: /OrganDonor/Assign?patientId=5&organ=Heart  ← organ pre-selected, matches loaded
    [HttpGet]
    public async Task<IActionResult> Assign(int patientId, string? organ)
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

        var model = new OrganDonorViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
            SelectedOrgan = organ,
        };

        if (!string.IsNullOrEmpty(organ))
        {
            try
            {
                List<TransplantMatch> matches = await transplantApiClient
                    .GetTopMatchesForDonorAsync(patientId, organ, HttpContext.RequestAborted);

                model.TopMatches = matches.Select(m => new TransplantMatchViewModel
                {
                    TransplantId = m.TransplantId,
                    ReceiverName = m.ReceiverName,
                    BloodType = m.BloodType,
                    CompatibilityScore = m.CompatibilityScore,
                    WaitingDays = m.WaitingDays,
                }).ToList();

                if (model.TopMatches.Count == 0)
                {
                    model.StatusMessage = $"No compatible recipients found for {organ}.";
                }
            }
            catch (InvalidOperationException ex)
            {
                model.ErrorMessage = ex.Message;
            }
        }

        return View(model);
    }

    // POST: /OrganDonor/Confirm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int patientId, int transplantId, float compatibilityScore)
    {
        try
        {
            await transplantApiClient.AssignDonorAsync(
                transplantId, patientId, compatibilityScore, HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Organ donor assignment confirmed successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Index", "Admin", new { selectedId = patientId });
    }
}