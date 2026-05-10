using System.Net;
using Common.API.Services;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/er-visits")]
    public class ERVisitsController : ControllerBase
    {
        private readonly IERVisitService _erVisitService;
        private readonly ILogger<ERVisitsController> _logger;

        public ERVisitsController(IERVisitService erVisitService, ILogger<ERVisitsController> logger)
        {
            _erVisitService = erVisitService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ER_Visit>>> GetAll()
        {
            try
            {
                var result = await _erVisitService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch ER visits.");

                return Problem(
                    detail: "Failed to fetch ER visits.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch ER visits.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ER_Visit>> GetById(int id)
        {
            try
            {
                ER_Visit? result = await _erVisitService.GetByIdAsync(id);
                if (result is null)
                {
                    _logger.LogWarning("ER visit {VisitId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch ER visit {VisitId}.", id);

                return Problem(
                    detail: "Failed to fetch ER visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch ER visit.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ER_Visit>> Create([FromBody] ER_Visit visit)
        {
            try
            {
                ER_Visit result = await _erVisitService.CreateAsync(visit);
                return CreatedAtAction(nameof(GetById), new { id = result.Visit_ID }, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create ER visit.");

                return Problem(
                    detail: "Failed to create ER visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create ER visit.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ER_Visit visit)
        {
            try
            {
                bool updated = await _erVisitService.UpdateAsync(id, visit);
                if (!updated)
                {
                    _logger.LogWarning("ER visit {VisitId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update ER visit {VisitId}.", id);

                return Problem(
                    detail: "Failed to update ER visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update ER visit.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await _erVisitService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("ER visit {VisitId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete ER visit {VisitId}.", id);

                return Problem(
                    detail: "Failed to delete ER visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete ER visit.");
            }
        }
    }
}
