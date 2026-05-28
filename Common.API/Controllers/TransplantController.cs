using System.Net;
using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/transplants")]
    [AuthorizeRole("Admin", "Medic")]
    public class TransplantController : ControllerBase
    {
        private readonly ITransplantService transplantService;
        private readonly ILogger<TransplantController> logger;

        public TransplantController(ITransplantService transplantService, ILogger<TransplantController> logger) : base()
        {
            this.transplantService = transplantService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Transplant>>> GetAll()
        {
            try
            {
                var result = await transplantService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch transplants.");

                return Problem(
                    detail: "Failed to fetch transplants.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplants.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transplant>> GetById(int id)
        {
            try
            {
                var result = await transplantService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("Transplant {TransplantId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch transplant with id {Id}.", id);
                return Problem(
                    detail: $"Failed to fetch transplant with id {id}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplant.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Transplant>> Create([FromBody] Transplant transplant)
        {
            try
            {
                Transplant result = await transplantService.CreateAsync(transplant);
                return CreatedAtAction(nameof(GetById), new { id = result.TransplantId }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create transplant.");

                return Problem(
                    detail: "Failed to create transplant.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create transplant.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Transplant transplant)
        {
            try
            {
                bool updated = await transplantService.UpdateAsync(id, transplant);
                if (!updated)
                {
                    logger.LogWarning("Transplant {TransplantId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update transplant {TransplantId}.", id);

                return Problem(
                    detail: "Failed to update transplant.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update transplant.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await transplantService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("Transplant {TransplantId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete transplant {TransplantId}.", id);

                return Problem(
                    detail: "Failed to delete transplant.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete transplant.");
            }
        }

        [HttpGet("receiver/{receiverId}")]
        public async Task<ActionResult<List<Transplant>>> GetByReceiverId(int receiverId)
        {
            try
            {
                var result = await transplantService.GetByReceiverIdAsync(receiverId);
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch transplants for receiver {ReceiverId}.", receiverId);
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
                var result = await transplantService.GetByDonorIdAsync(donorId);
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch transplants for donor {DonorId}.", donorId);
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
                var result = await transplantService.GetTopMatchesAsDisplayModelsAsync(donorId, organType);
                return Ok(result);
            }
            catch (InvalidOperationException e)
            {
                logger.LogWarning(e, "Invalid donor state for donor {DonorId}.", donorId);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch top matches for donor {DonorId}.", donorId);
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
                var result = await transplantService.IsUrgentAsync(patientId);
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to check urgency for patient {PatientId}.", patientId);
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
                var result = await transplantService.GetChronicWarningAsync(patientId);
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to get chronic warning for patient {PatientId}.", patientId);
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
                await transplantService.CreateWaitlistRequestAsync(dto.ReceiverId, dto.OrganType);
                return Ok();
            }
            catch (ArgumentException e)
            {
                logger.LogWarning(e, "Invalid receiver for waitlist request.");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to create waitlist request for receiver {ReceiverId}.", dto.ReceiverId);
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
                await transplantService.AssignDonorAsync(id, dto.DonorId, dto.FinalScore);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to assign donor for transplant {TransplantId}.", id);
                return Problem(
                    detail: $"Failed to assign donor for transplant {id}.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not assign donor.");
            }
        }
    }
}
