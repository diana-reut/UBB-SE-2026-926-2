using HospitalManagement.Web.Models.Statistics;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class StatisticsController : Controller
{
    private readonly IStatisticsApiClient statisticsApiClient;

    public StatisticsController(IStatisticsApiClient statisticsApiClient)
    {
        this.statisticsApiClient = statisticsApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? type)
    {
        StatisticsType selectedType = StatisticsViewModel.FromKey(type);
        StatisticsModel model = await BuildStatisticsModelAsync(selectedType, HttpContext.RequestAborted);

        return View(StatisticsViewModel.FromModel(model));
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
