using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/transfer-logs")]
    [AuthorizeRole("Admin", "Medic")]
    public class TransferLogController : ControllerBase
    {
        private readonly ITransferLogService transferLogService;
        private readonly ILogger<TransferLogController> logger;

        public TransferLogController(ITransferLogService transferLogService, ILogger<TransferLogController> logger)
        {
            this.transferLogService = transferLogService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Transfer_Log>>> GetAll()
        {
            try
            {
                var result = await transferLogService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch transfer logs.");

                return Problem(
                    detail: "Failed to fetch transfer logs.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transfer logs.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Transfer_Log>> GetById(int id)
        {
            try
            {
                Transfer_Log? result = await transferLogService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("Transfer log {TransferId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch transfer log {TransferId}.", id);

                return Problem(
                    detail: "Failed to fetch transfer log.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transfer log.");
            }
        }

        [HttpGet("visit/{visitId:int}")]
        public async Task<ActionResult<List<Transfer_Log>>> GetByVisitId(int visitId)
        {
            try
            {
                return Ok(await transferLogService.GetByVisitIdAsync(visitId));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch transfer logs for visit {VisitId}.", visitId);
                return Problem(
                    detail: "Failed to fetch transfer logs for visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch transfer logs.");
            }
        }

        [HttpGet("eligible-visits")]
        public async Task<ActionResult<List<ERTransferEligibleVisitDto>>> GetEligibleVisits()
        {
            try
            {
                return Ok(await transferLogService.GetEligibleVisitsAsync());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch eligible transfer visits.");
                return Problem(
                    detail: "Failed to fetch eligible transfer visits.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch eligible transfer visits.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Transfer_Log>> Create([FromBody] Transfer_Log transferLog)
        {
            try
            {
                Transfer_Log result = await transferLogService.CreateAsync(transferLog);
                return CreatedAtAction(nameof(GetById), new { id = result.Transfer_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create transfer log.");

                return Problem(
                    detail: "Failed to create transfer log.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create transfer log.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Transfer_Log transferLog)
        {
            try
            {
                bool updated = await transferLogService.UpdateAsync(id, transferLog);
                if (!updated)
                {
                    logger.LogWarning("Transfer log {TransferId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update transfer log {TransferId}.", id);

                return Problem(
                    detail: "Failed to update transfer log.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update transfer log.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await transferLogService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("Transfer log {TransferId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete transfer log {TransferId}.", id);

                return Problem(
                    detail: "Failed to delete transfer log.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete transfer log.");
            }
        }
    }
}
