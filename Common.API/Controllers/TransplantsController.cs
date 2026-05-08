using System.Net;
using Common.API.Services;
using Common.Data.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/transplants")]
    public class TransplantsController : ControllerBase
    {
        private readonly ITransplantsService _transplantsService;
        private readonly ILogger<TransplantsController> _logger;

        public TransplantsController(ITransplantsService transplantsService, ILogger<TransplantsController> logger)
        {
            _transplantsService = transplantsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Transplant>>> GetAll()
        {
            try
            {
                var result = await _transplantsService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch transplants.");

                return Problem(
                    detail: "Failed to fetch transplants.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplants.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Transplant>> GetById(int id)
        {
            try
            {
                Transplant? result = await _transplantsService.GetByIdAsync(id);
                if (result is null)
                {
                    _logger.LogWarning("Transplant {TransplantId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch transplant {TransplantId}.", id);

                return Problem(
                    detail: "Failed to fetch transplant.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transplant.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Transplant>> Create([FromBody] Transplant transplant)
        {
            try
            {
                Transplant result = await _transplantsService.CreateAsync(transplant);
                return CreatedAtAction(nameof(GetById), new { id = result.TransplantId }, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create transplant.");

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
                bool updated = await _transplantsService.UpdateAsync(id, transplant);
                if (!updated)
                {
                    _logger.LogWarning("Transplant {TransplantId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update transplant {TransplantId}.", id);

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
                bool deleted = await _transplantsService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Transplant {TransplantId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete transplant {TransplantId}.", id);

                return Problem(
                    detail: "Failed to delete transplant.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete transplant.");
            }
        }
    }
}
