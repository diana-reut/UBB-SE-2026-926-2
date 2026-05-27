using HospitalManagement.Web.Models.Ghost;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class GhostController : Controller
{
    private readonly IGhostApiClient ghostApiClient;

    public GhostController(IGhostApiClient ghostApiClient)
    {
        this.ghostApiClient = ghostApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        GhostModel model = await LoadModelAsync();
        return View(GhostViewModel.FromModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportSighting()
    {
        try
        {
            GhostStatusDto status = await ghostApiClient.ReportSightingAsync(HttpContext.RequestAborted);
            var model = BuildModel(status);
            model.StatusMessage = status.exorcismTriggered
                ? "Exorcism triggered! More than 3 sightings in the last 24 hours."
                : $"Sighting recorded. Total in last 24 h: {status.sightingCount}.";
            return View("Index", GhostViewModel.FromModel(model));
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Could not report sighting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<GhostModel> LoadModelAsync()
    {
        try
        {
            GhostStatusDto status = await ghostApiClient.GetExorcismStatusAsync(HttpContext.RequestAborted);
            return BuildModel(status);
        }
        catch
        {
            return new GhostModel { StatusMessage = "Could not reach the ghost service." };
        }
    }

    private static GhostModel BuildModel(GhostStatusDto status) =>
        new ()
        {
            ExorcismTriggered = status.exorcismTriggered,
            SightingCount = status.sightingCount,
            LastRefreshed = DateTime.UtcNow
        };
}
