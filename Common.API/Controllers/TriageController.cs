using System.Net;
using Common.API.Services;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/triages")]
    public class TriageController : ControllerBase
    {
        private readonly ITriageService _triageService;
        private readonly ILogger<TriageController> _logger;

        public TriageController(ITriageService triageService, ILogger<TriageController> logger)
        {
            _triageService = triageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Triage>>> GetAll()
        {
            try
            {
                var result = await _triageService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch triages.");

                return Problem(
                    detail: "Failed to fetch triages.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triages.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Triage>> GetById(int id)
        {
            try
            {
                Triage? result = await _triageService.GetByIdAsync(id);
                if (result is null)
                {
                    _logger.LogWarning("Triage {TriageId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to fetch triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triage.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Triage>> Create([FromBody] Triage triage)
        {
            try
            {
                Triage result = await _triageService.CreateAsync(triage);
                return CreatedAtAction(nameof(GetById), new { id = result.Triage_ID }, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create triage.");

                return Problem(
                    detail: "Failed to create triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create triage.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Triage triage)
        {
            try
            {
                bool updated = await _triageService.UpdateAsync(id, triage);
                if (!updated)
                {
                    _logger.LogWarning("Triage {TriageId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to update triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update triage.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await _triageService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Triage {TriageId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to delete triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete triage.");
            }
        }
    }
}
