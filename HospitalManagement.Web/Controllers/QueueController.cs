using Common.Data.Models;
using HospitalManagement.Web.Models.Queue;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class QueueController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public QueueController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            List<ER_Visit> waitingVisits = await erApiClient.GetVisitsByStatusAsync(
                ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                cancellationToken);
            List<Triage> triages = await erApiClient.GetTriagesAsync(cancellationToken);
            List<Triage_Parameters> triageParameters = await erApiClient.GetTriageParametersAsync(cancellationToken);

            var model = new QueueViewModel
            {
                ActiveVisits = waitingVisits
                    .Join(
                        triages.Where(triage => triageParameters.Any(parameters => parameters.Triage_ID == triage.Triage_ID)),
                        visit => visit.Visit_ID,
                        triage => triage.Visit_ID,
                        (visit, triage) => new QueueItemViewModel
                        {
                            VisitId = visit.Visit_ID,
                            PatientId = visit.Patient_ID,
                            TriageLevel = triage.Triage_Level,
                            Specialization = triage.Specialization,
                            ArrivalTime = visit.Arrival_date_time,
                            Status = visit.Status
                        })
                    .OrderBy(item => item.TriageLevel)
                    .ThenBy(item => item.ArrivalTime)
                    .ToList()
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new QueueViewModel { ErrorMessage = ex.Message });
        }
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening the ER queue.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
