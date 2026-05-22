using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Models.Transfer;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class TransferController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public TransferController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            TransferViewModel model = await BuildModelAsync(selectedVisitId, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new TransferViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.TransferVisitAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} was transferred to Patient Management.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Transfer failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retry(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.RetryTransferAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Transfer retry for visit {visitId} succeeded.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Retry failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.CloseVisitAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} was closed.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Close failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<TransferViewModel> BuildModelAsync(int? selectedVisitId, CancellationToken cancellationToken)
    {
        List<ERTransferEligibleVisitDto> eligibleVisits = await erApiClient.GetEligibleTransferVisitsAsync(cancellationToken);
        var model = new TransferViewModel
        {
            SelectedVisitId = selectedVisitId,
            EligibleVisits = eligibleVisits
                .Select(visit => new TransferVisitViewModel
                {
                    VisitId = visit.Visit_ID,
                    PatientName = visit.PatientName,
                    ChiefComplaint = visit.Chief_Complaint,
                    Status = visit.Status,
                    Transferred = visit.Transferred
                })
                .ToList()
        };

        if (!selectedVisitId.HasValue)
        {
            return model;
        }

        List<Transfer_Log> logs = await erApiClient.GetTransferLogsByVisitIdAsync(selectedVisitId.Value, cancellationToken);
        model.TransferLogs = logs
            .Select(log => new TransferLogItemViewModel
            {
                TransferId = log.Transfer_ID,
                VisitId = log.Visit_ID,
                TransferTime = log.Transfer_Time,
                TargetSystem = log.Target_System,
                Status = log.Status
            })
            .ToList();
        model.CanRetry = model.TransferLogs.FirstOrDefault()?.Status == "FAILED";

        return model;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening transfers.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
