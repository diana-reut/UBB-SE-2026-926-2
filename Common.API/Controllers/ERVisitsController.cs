using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/er-visits")]
    [AuthorizeRole("Admin", "Medic")]
    public class ERVisitsController : ControllerBase
    {
        private readonly IERVisitService erVisitService;
        private readonly ILogger<ERVisitsController> logger;

        public ERVisitsController(IERVisitService erVisitService, ILogger<ERVisitsController> logger)
        {
            this.erVisitService = erVisitService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ER_Visit>>> GetAll()
        {
            try
            {
                var result = await erVisitService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch ER visits.");

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
                ER_Visit? result = await erVisitService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("ER visit {VisitId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch ER visit {VisitId}.", id);

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
                ER_Visit result = await erVisitService.CreateAsync(visit);
                return CreatedAtAction(nameof(GetById), new { id = result.Visit_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create ER visit.");

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
                bool updated = await erVisitService.UpdateAsync(id, visit);
                if (!updated)
                {
                    logger.LogWarning("ER visit {VisitId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update ER visit {VisitId}.", id);

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
                bool deleted = await erVisitService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("ER visit {VisitId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete ER visit {VisitId}.", id);

                return Problem(
                    detail: "Failed to delete ER visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete ER visit.");
            }
        }

        [HttpPost("auto-assign-room")]
        public async Task<ActionResult<bool>> AutoAssignRoom()
        {
            try
            {
                bool assigned = await erVisitService.AutoAssignHighestPriorityRoomAsync();
                return Ok(assigned);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to auto-assign room.");
                return Problem(
                    detail: "Failed to auto-assign room.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not auto-assign room.");
            }
        }

        [HttpPost("{visitId:int}/assign-room/{roomId:int}")]
        public async Task<IActionResult> AssignRoom(int visitId, int roomId)
        {
            try
            {
                await erVisitService.AssignRoomAsync(visitId, roomId);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to assign room {RoomId} to visit {VisitId}.", roomId, visitId);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not assign room.");
            }
        }

        [HttpPost("{visitId:int}/transfer")]
        public async Task<IActionResult> Transfer(int visitId)
        {
            try
            {
                await erVisitService.TransferVisitAsync(visitId);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to transfer visit {VisitId}.", visitId);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not transfer visit.");
            }
        }

        [HttpPost("{visitId:int}/retry-transfer")]
        public async Task<IActionResult> RetryTransfer(int visitId)
        {
            try
            {
                await erVisitService.RetryTransferAsync(visitId);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to retry transfer for visit {VisitId}.", visitId);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not retry transfer.");
            }
        }

        [HttpPost("{visitId:int}/close")]
        public async Task<IActionResult> Close(int visitId)
        {
            try
            {
                await erVisitService.CloseVisitAsync(visitId);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to close visit {VisitId}.", visitId);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not close visit.");
            }
        }
    }
}
