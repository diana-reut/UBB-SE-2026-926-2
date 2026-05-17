using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Auth;

public static class ControllerBaseExtensions
{
    public static UserPrincipal GetCurrentUser(this ControllerBase controller)
    {
        ClaimsPrincipal claims = controller.User;

        string? rawId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

        _ = int.TryParse(rawId, out int id);

        string username = claims.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? claims.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        string role = claims.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return new UserPrincipal { Id = id, Username = username, Role = role };
    }
}
