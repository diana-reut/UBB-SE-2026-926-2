using System.Net;
using Common.API.Auth;
using Common.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Common.Data;
using Common.API.Service;
using Common.Data.Integration;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/prescriptions")]
    [AuthorizeRole("Admin", "Medic")]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionController> _logger;

        public PrescriptionController(IPrescriptionService prescriptionService, ILogger<PrescriptionController> logger) : base()
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        [HttpGet("latest")]
        public async Task<ActionResult<List<Prescription>>> GetLatestPrescriptions([FromQuery] GetLatestPrescriptionsDTO dto)
        {
            try
            {
                var result = await _prescriptionService.GetLatestPrescriptionsAsync(dto.N, dto.Page);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch latest prescriptions.");
                return Problem(
                    detail: "Failed to fetch latest prescriptions.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch prescriptions.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<Prescription>>> GetPrescriptions([FromBody] PrescriptionFilter? filter)
        {
            try
            {
                var result = await _prescriptionService.ApplyFilterAsync(filter ?? new PrescriptionFilter());
                return Ok(result);
            }
            catch (MyNotImplementedException e)
            {
                _logger.LogWarning(e, "Prescription filter operation is currently unavailable.");
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.ServiceUnavailable,
                    title: "Filter operation currently unavailable.");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch prescriptions.");
                return Problem(
                    detail: "Failed to fetch prescriptions.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch prescriptions.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Prescription>> GetPrescriptionDetails(int id)
        {
            try
            {
                var result = await _prescriptionService.GetPrescriptionDetailsAsync(id);
                return Ok(result);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e, "Prescription with ID {Id} not found.", id);
                return Problem(
                    detail: $"Prescription with ID {id} does not exist.",
                    statusCode: (int)HttpStatusCode.NotFound,
                    title: "Prescription not found.");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch prescription with ID {Id}.", id);
                return Problem(
                    detail: "Failed to fetch prescription details.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch prescription.");
            }
        }
    }
}


