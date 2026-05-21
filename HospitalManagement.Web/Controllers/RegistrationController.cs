using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Models.Registration;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class RegistrationController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IErWorkflowApiClient erApiClient;

    public RegistrationController(IPatientApiClient patientApiClient, IErWorkflowApiClient erApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new RegistrationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistrationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            string patientId = model.PatientId.Trim();
            bool patientExists = await patientApiClient.ExistsAsync(patientId, cancellationToken);

            if (!patientExists)
            {
                Patient created = await patientApiClient.CreatePatientAsync(new CreatePatientDto
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Cnp = patientId,
                    Dob = model.DateOfBirth,
                    Sex = model.Sex,
                    PhoneNo = model.Phone.Trim(),
                    EmergencyContact = model.EmergencyContact.Trim(),
                    IsDonor = false
                }, cancellationToken);

                TempData["SuccessMessage"] = $"Patient {created.FullName} was created.";
            }

            ER_Visit visit = await erApiClient.CreateVisitAsync(new ER_Visit
            {
                Patient_ID = patientId,
                Chief_Complaint = model.ChiefComplaint.Trim(),
                Arrival_date_time = DateTime.Now,
                Status = ER_Visit.VisitStatus.REGISTERED
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Registration complete. Visit {visit.Visit_ID} is ready for triage.";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", model);
        }
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before registering ER patients.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
