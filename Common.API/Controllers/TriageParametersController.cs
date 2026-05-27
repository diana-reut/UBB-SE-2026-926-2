using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/triage-parameters")]
    [AuthorizeRole("Admin", "Medic")]
    public class TriageParametersController : ControllerBase
    {
        private readonly ITriageParametersService triageParametersService;
        private readonly ILogger<TriageParametersController> logger;

        public TriageParametersController(
            ITriageParametersService triageParametersService,
            ILogger<TriageParametersController> logger)
        {
            this.triageParametersService = triageParametersService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Triage_Parameters>>> GetAll()
        {
            try
            {
                var result = await triageParametersService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch triage parameters.");

                return Problem(
                    detail: "Failed to fetch triage parameters.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triage parameters.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Triage_Parameters>> GetById(int id)
        {
            try
            {
                Triage_Parameters? result = await triageParametersService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("Triage parameters {TriageId} were not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch triage parameters {TriageId}.", id);

                return Problem(
                    detail: "Failed to fetch triage parameters.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triage parameters.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Triage_Parameters>> Create([FromBody] Triage_Parameters parameters)
        {
            try
            {
                Triage_Parameters result = await triageParametersService.CreateAsync(parameters);
                return CreatedAtAction(nameof(GetById), new { id = result.Triage_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create triage parameters.");

                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create triage parameters.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Triage_Parameters parameters)
        {
            try
            {
                bool updated = await triageParametersService.UpdateAsync(id, parameters);
                if (!updated)
                {
                    logger.LogWarning("Triage parameters {TriageId} were not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update triage parameters {TriageId}.", id);

                return Problem(
                    detail: "Failed to update triage parameters.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update triage parameters.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await triageParametersService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("Triage parameters {TriageId} were not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete triage parameters {TriageId}.", id);

                return Problem(
                    detail: "Failed to delete triage parameters.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete triage parameters.");
            }
        }
    }
}
