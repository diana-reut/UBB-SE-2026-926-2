using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [AuthorizeRole("Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService statisticsService;
        private readonly ILogger<StatisticsController> logger;

        public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
        {
            this.statisticsService = statisticsService;
            this.logger = logger;
        }

        [HttpGet("active-vs-archived")]
        public async Task<ActionResult<Dictionary<string, int>>> GetActiveVsArchivedRatio()
        {
            try
            {
                var result = await statisticsService.GetActiveVsArchivedRatioAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch Active vs Archived ratio.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch Active/Archived statistics.");
            }
        }

        [HttpGet("age-distribution")]
        public async Task<ActionResult<Dictionary<string, int>>> GetAgeDistribution()
        {
            try
            {
                var result = await statisticsService.GetAgeDistributionAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch age distribution.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch age distribution.");
            }
        }

        [HttpGet("blood-types")]
        public async Task<ActionResult<Dictionary<string, int>>> GetPatientsByBloodType()
        {
            try
            {
                var result = await statisticsService.GetPatientsByBloodTypeAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch blood type distribution.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch blood type statistics.");
            }
        }

        [HttpGet("rh-factor")]
        public async Task<ActionResult<Dictionary<string, int>>> GetPatientsByRh()
        {
            try
            {
                var result = await statisticsService.GetPatientsByRhAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch Rh factor distribution.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch Rh factor statistics.");
            }
        }

        [HttpGet("gender-distribution")]
        public async Task<ActionResult<Dictionary<string, int>>> GetGenderDistribution()
        {
            try
            {
                var result = await statisticsService.GetPatientGenderDistributionAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch gender distribution.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch gender statistics.");
            }
        }

        [HttpGet("consultations")]
        public async Task<ActionResult<Dictionary<string, int>>> GetConsultationDistribution()
        {
            try
            {
                var result = await statisticsService.GetConsultationDistributionAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch consultation distribution.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch consultation statistics.");
            }
        }

        [HttpGet("top-diagnoses")]
        public async Task<ActionResult<Dictionary<string, int>>> GetTopDiagnoses()
        {
            try
            {
                var result = await statisticsService.GetTopDiagnosesAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch top diagnoses.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch diagnosis statistics.");
            }
        }

        [HttpGet("top-meds")]
        public async Task<ActionResult<Dictionary<string, int>>> GetMostPrescribedMeds()
        {
            try
            {
                var result = await statisticsService.GetMostPrescribedMedsAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch most prescribed meds.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch medication statistics.");
            }
        }
    }
}
