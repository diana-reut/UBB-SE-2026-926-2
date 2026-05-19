using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class AuthenticationController : Controller
{
    private const string AccessTokenSessionKey = "AccessToken";
    private const string UsernameSessionKey = "Username";
    private const string RoleSessionKey = "Role";

    private readonly IAuthenticationApiClient authenticationApiClient;

    public AuthenticationController(IAuthenticationApiClient authenticationApiClient)
    {
        this.authenticationApiClient = authenticationApiClient;
    }

    [HttpGet]
    public IActionResult AuthenticationView()
    {
        return View(new AuthenticationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AuthenticationView(
        AuthenticationViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            Common.Data.Entity.DTOs.AuthResponseDto response =
                await authenticationApiClient.LoginAsync(model.Username.Trim(), model.Password, cancellationToken);

            HttpContext.Session.SetString(AccessTokenSessionKey, response.Token);
            HttpContext.Session.SetString(UsernameSessionKey, response.Username);
            HttpContext.Session.SetString(RoleSessionKey, response.Role);

            TempData["LoginMessage"] = $"Signed in as {response.Username} ({response.Role}).";
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException e)
        {
            model.ErrorMessage = e.Message;
            ModelState.AddModelError(string.Empty, e.Message);
            return View(model);
        }
        catch (HttpRequestException)
        {
            const string message = "Could not connect to the authentication API.";
            model.ErrorMessage = message;
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
        catch (TaskCanceledException)
        {
            const string message = "The authentication request timed out.";
            model.ErrorMessage = message;
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
        catch (Exception e)
        {
            model.ErrorMessage = e.Message;
            ModelState.AddModelError(string.Empty, e.Message);
            return View(model);
        }
    }
}
