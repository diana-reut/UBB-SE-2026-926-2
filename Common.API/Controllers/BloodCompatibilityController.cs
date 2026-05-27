using System.Net;
using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/bloodcompatibilities")]
[AuthorizeRole("Admin", "Medic")]
public class BloodCompatibilityController : ControllerBase
{
    private readonly IBloodCompatibilityService service;
    private readonly ILogger<BloodCompatibilityController> logger;

    public BloodCompatibilityController(
        IBloodCompatibilityService service,
        ILogger<BloodCompatibilityController> logger)
    {
        this.service = service;
        this.logger = logger;
    }

    [HttpPost("top-donors")]
    public async Task<ActionResult<List<Patient>>> GetTopCompatibleDonors(
        [FromBody] GetTopCompatibleDonorsDto dto)
    {
        try
        {
            var result =
                await service.GetTopCompatibleDonorsAsync(dto.RecipientId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to compute compatible donors.");

            return Problem(
                detail: "Failed to compute compatible donors.",
                title: "Blood compatibility error",
                statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
