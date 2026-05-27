using System;
using System.Collections.Generic;
using System.Text;
using HospitalManagement.Web.Models.Transplant;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class TransplantController : Controller
{
    private readonly ITransplantApiClient transplantApiClient;
    private readonly IPatientApiClient patientApiClient;

    public TransplantController(ITransplantApiClient transplantApiClient, IPatientApiClient patientApiClient)
    {
        this.transplantApiClient = transplantApiClient;
        this.patientApiClient = patientApiClient;
    }

    // GET: /Transplant/Request?patientId=5
    [HttpGet]
    public async Task<IActionResult> Request(int patientId)
    {
        var patient = await patientApiClient.GetByIdAsync(patientId, HttpContext.RequestAborted);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction("Index", "Admin");
        }

        bool isUrgent;
        string? warning;

        try
        {
            isUrgent = await transplantApiClient.IsUrgentAsync(patientId, HttpContext.RequestAborted);
            warning = await transplantApiClient.GetChronicWarningAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            isUrgent = false;
            warning = null;
        }

        var model = new TransplantRequestViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
            IsUrgent = isUrgent,
            WarningMessage = warning,
        };

        return View(model);
    }

    // POST: /Transplant/Request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(TransplantRequestViewModel model)
    {
        // Re-hydrate read-only display fields before returning the view on error
        async Task<IActionResult> ReturnWithErrors()
        {
            var patient = await patientApiClient.GetByIdAsync(model.PatientId, HttpContext.RequestAborted);
            model.PatientName = patient?.FullName ?? string.Empty;

            try
            {
                model.IsUrgent = await transplantApiClient.IsUrgentAsync(model.PatientId, HttpContext.RequestAborted);
                model.WarningMessage = await transplantApiClient.GetChronicWarningAsync(model.PatientId, HttpContext.RequestAborted);
            }
            catch
            { /* non-critical display data */
            }

            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return await ReturnWithErrors();
        }

        try
        {
            await transplantApiClient.CreateWaitlistRequestAsync(
                model.PatientId, model.SelectedOrgan!, HttpContext.RequestAborted);

            TempData["SuccessMessage"] =
                "The patient has been successfully added to the Organ Transplant Waitlist.";

            return RedirectToAction("Index", "Admin", new { selectedId = model.PatientId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnWithErrors();
        }
    }
}