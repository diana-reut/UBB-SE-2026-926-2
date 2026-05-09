using System.Net;
using Common.API.Services;
using Common.Data.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/transplants")]
    public class TransplantController : ControllerBase
    {
        private readonly ITransplantService _transplantService;
        private readonly ILogger<TransplantController> _logger;

        public TransplantController(ITransplantService transplantService, ILogger<TransplantController> logger) : base()
        {
            _transplantService = transplantService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transplant>> GetById(int id)
        {
            try
            {
                var result = await _transplantService.GetByIdAsync(id);
                if (result is null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch transplant with id {Id}.", id);
                return Problem(
                    detail: $"Failed to fetch transplant with id {id}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplant.");
            }
        }

        [HttpGet("receiver/{receiverId}")]
        public async Task<ActionResult<List<Transplant>>> GetByReceiverId(int receiverId)
        {
            try
            {
                var result = await _transplantService.GetByReceiverIdAsync(receiverId);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch transplants for receiver {ReceiverId}.", receiverId);
                return Problem(
                    detail: $"Failed to fetch transplants for receiver {receiverId}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplants.");
            }
        }

        [HttpGet("donor/{donorId}")]
        public async Task<ActionResult<List<Transplant>>> GetByDonorId(int donorId)
        {
            try
            {
                var result = await _transplantService.GetByDonorIdAsync(donorId);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch transplants for donor {DonorId}.", donorId);
                return Problem(
                    detail: $"Failed to fetch transplants for donor {donorId}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplants.");
            }
        }

        [HttpGet("matches/donor/{donorId}")]
        public async Task<ActionResult<List<TransplantMatch>>> GetTopMatchesForDonor(int donorId, [FromQuery] string organType)
        {
            try
            {
                var result = await _transplantService.GetTopMatchesAsDisplayModelsAsync(donorId, organType);
                return Ok(result);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e, "Invalid donor state for donor {DonorId}.", donorId);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to fetch top matches for donor {DonorId}.", donorId);
                return Problem(
                    detail: $"Failed to fetch top matches for donor {donorId}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplant matches.");
            }
        }

        [HttpGet("urgent/{patientId}")]
        public async Task<ActionResult<bool>> IsUrgent(int patientId)
        {
            try
            {
                var result = await _transplantService.IsUrgentAsync(patientId);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to check urgency for patient {PatientId}.", patientId);
                return Problem(
                    detail: $"Failed to check urgency for patient {patientId}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not check urgency.");
            }
        }

        [HttpGet("chronic-warning/{patientId}")]
        public async Task<ActionResult<string?>> GetChronicWarning(int patientId)
        {
            try
            {
                var result = await _transplantService.GetChronicWarningAsync(patientId);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to get chronic warning for patient {PatientId}.", patientId);
                return Problem(
                    detail: $"Failed to get chronic warning for patient {patientId}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not get chronic warning.");
            }
        }

        [HttpPost("waitlist")]
        public async Task<ActionResult> CreateWaitlistRequest([FromBody] CreateWaitlistRequestDto dto)
        {
            try
            {
                await _transplantService.CreateWaitlistRequestAsync(dto.ReceiverId, dto.OrganType);
                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e, "Invalid receiver for waitlist request.");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to create waitlist request for receiver {ReceiverId}.", dto.ReceiverId);
                return Problem(
                    detail: "Failed to create waitlist request.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create waitlist request.");
            }
        }

        [HttpPut("{id}/assign-donor")]
        public async Task<ActionResult> AssignDonor(int id, [FromBody] AssignDonorDto dto)
        {
            try
            {
                await _transplantService.AssignDonorAsync(id, dto.DonorId, dto.FinalScore);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to assign donor for transplant {TransplantId}.", id);
                return Problem(
                    detail: $"Failed to assign donor for transplant {id}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not assign donor.");
            }
        }
    }
}
