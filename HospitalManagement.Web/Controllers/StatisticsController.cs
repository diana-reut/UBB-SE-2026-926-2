using HospitalManagement.Web.Models.Statistics;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class StatisticsController : Controller
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IStatisticsApiClient statisticsApiClient;
    private readonly IMemoryCache cache;

    public StatisticsController(IStatisticsApiClient statisticsApiClient, IMemoryCache cache)
    {
        this.statisticsApiClient = statisticsApiClient;
        this.cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? type)
    {
        StatisticsType selectedType = StatisticsViewModel.FromKey(type);
        StatisticsModel model = await GetOrBuildStatisticsAsync(selectedType, HttpContext.RequestAborted);

        return View(StatisticsViewModel.FromModel(model));
    }

    private async Task<StatisticsModel> GetOrBuildStatisticsAsync(
        StatisticsType type,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"statistics_{type}";
        if (cache.TryGetValue(cacheKey, out StatisticsModel? cached) && cached is not null)
        {
            return cached;
        }

        StatisticsModel model = await BuildStatisticsModelAsync(type, cancellationToken);
        cache.Set(cacheKey, model, CacheDuration);

        return model;
    }

    private async Task<StatisticsModel> BuildStatisticsModelAsync(
        StatisticsType type,
        CancellationToken cancellationToken)
    {
        var model = new StatisticsModel
        {
            SelectedType = type,
            CachedAt = DateTime.UtcNow,
        };

        try
        {
            switch (type)
            {
                case StatisticsType.ConsultationSource:
                    model.PrimaryData = await statisticsApiClient.GetConsultationDistributionAsync(cancellationToken);
                    break;
                case StatisticsType.TopDiagnoses:
                    model.PrimaryData = await statisticsApiClient.GetTopDiagnosesAsync(cancellationToken);
                    break;
                case StatisticsType.TopMedications:
                    model.PrimaryData = await statisticsApiClient.GetMostPrescribedMedsAsync(cancellationToken);
                    break;
                case StatisticsType.Demographics:
                    model.PrimaryData = await statisticsApiClient.GetPatientGenderDistributionAsync(cancellationToken);
                    model.SecondaryData = await statisticsApiClient.GetAgeDistributionAsync(cancellationToken);
                    break;
                default:
                    model.PrimaryData = await statisticsApiClient.GetActiveVsArchivedRatioAsync(cancellationToken);
                    break;
            }
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = $"Could not load statistics: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            model.ErrorMessage = "The statistics request timed out or was interrupted.";
        }

        return model;
    }
}
