using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/er-rooms")]
    [AuthorizeRole("Admin", "Medic")]
    public class ERRoomsController : ControllerBase
    {
        private readonly IERRoomService erRoomService;
        private readonly ILogger<ERRoomsController> logger;

        public ERRoomsController(IERRoomService erRoomService, ILogger<ERRoomsController> logger)
        {
            this.erRoomService = erRoomService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ER_Room>>> GetAll()
        {
            try
            {
                var result = await erRoomService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch ER rooms.");

                return Problem(
                    detail: "Failed to fetch ER rooms.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch ER rooms.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ER_Room>> GetById(int id)
        {
            try
            {
                ER_Room? result = await erRoomService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("ER room {RoomId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch ER room {RoomId}.", id);

                return Problem(
                    detail: "Failed to fetch ER room.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch ER room.");
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<ER_Room>>> GetByStatus(string status)
        {
            try
            {
                return Ok(await erRoomService.GetByStatusAsync(status));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch ER rooms with status {Status}.", status);
                return Problem(
                    detail: "Failed to fetch ER rooms by status.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch ER rooms by status.");
            }
        }

        [HttpGet("{id:int}/visit-details")]
        public async Task<ActionResult<ERRoomVisitDetailsDto>> GetVisitDetails(int id)
        {
            try
            {
                ERRoomVisitDetailsDto? result = await erRoomService.GetVisitDetailsAsync(id);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch visit details for room {RoomId}.", id);
                return Problem(
                    detail: "Failed to fetch room visit details.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch room visit details.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ER_Room>> Create([FromBody] ER_Room room)
        {
            try
            {
                ER_Room result = await erRoomService.CreateAsync(room);
                return CreatedAtAction(nameof(GetById), new { id = result.Room_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create ER room.");

                return Problem(
                    detail: "Failed to create ER room.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create ER room.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ER_Room room)
        {
            try
            {
                bool updated = await erRoomService.UpdateAsync(id, room);
                if (!updated)
                {
                    logger.LogWarning("ER room {RoomId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update ER room {RoomId}.", id);

                return Problem(
                    detail: "Failed to update ER room.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update ER room.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await erRoomService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("ER room {RoomId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete ER room {RoomId}.", id);

                return Problem(
                    detail: "Failed to delete ER room.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete ER room.");
            }
        }

        [HttpPost("{id:int}/mark-cleaning")]
        public async Task<IActionResult> MarkCleaning(int id)
        {
            try
            {
                await erRoomService.MarkRoomAsCleaningAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to mark room {RoomId} as cleaning.", id);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not mark room as cleaning.");
            }
        }

        [HttpPost("{id:int}/mark-available")]
        public async Task<IActionResult> MarkAvailable(int id)
        {
            try
            {
                await erRoomService.MarkRoomAsAvailableAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to mark room {RoomId} as available.", id);
                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not mark room as available.");
            }
        }
    }
}
