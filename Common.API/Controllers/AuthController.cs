using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly ILogger<AuthController> logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            AuthResponseDto response = await authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogWarning(e, "Login failed for user {Username}.", dto.Username);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unexpected error during login for user {Username}.", dto.Username);
            return Problem(
                detail: "Login failed due to an unexpected error.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Login failed.");
        }
    }

    [HttpPost("register")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            AuthResponseDto response = await authService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Registration failed for user {Username}.", dto.Username);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unexpected error during registration for user {Username}.", dto.Username);
            return Problem(
                detail: "Registration failed due to an unexpected error.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Registration failed.");
        }
    }
}
