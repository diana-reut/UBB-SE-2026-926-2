using Common.API.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Common.API.Controllers;

[ApiController]
[Route("api/ghost")]
[AuthorizeRole("Admin", "Doctor", "Nurse")]
public class GhostController : ControllerBase
{
    private const string SightingsCacheKey = "ghost_sightings";
    private readonly IMemoryCache cache;

    public GhostController(IMemoryCache cache)
    {
        this.cache = cache;
    }

    [HttpPost("sighting")]
    public IActionResult ReportSighting()
    {
        List<DateTime> sightings = GetActiveSightings();
        sightings.Add(DateTime.UtcNow);
        SaveSightings(sightings);

        bool exorcismTriggered = sightings.Count > 3;
        return Ok(new { ExorcismTriggered = exorcismTriggered, SightingCount = sightings.Count });
    }

    [HttpGet("exorcism-status")]
    public IActionResult GetExorcismStatus()
    {
        List<DateTime> sightings = GetActiveSightings();
        bool triggered = sightings.Count > 3;
        return Ok(new { ExorcismTriggered = triggered, SightingCount = sightings.Count });
    }

    private List<DateTime> GetActiveSightings()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var all = cache.Get<List<DateTime>>(SightingsCacheKey) ?? new List<DateTime>();
        return all.Where(s => s >= cutoff).ToList();
    }

    private void SaveSightings(List<DateTime> sightings)
    {
        // Slide the cache window to always expire 24 h after the latest sighting
        cache.Set(SightingsCacheKey, sightings, TimeSpan.FromHours(24));
    }
}
