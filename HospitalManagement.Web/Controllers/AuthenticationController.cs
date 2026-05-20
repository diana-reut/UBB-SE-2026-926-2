using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.Web.Controllers;

public class AuthenticationController : Controller
{
    private readonly IAuthenticationApiClient authenticationApiClient;

    public AuthenticationController(IAuthenticationApiClient authenticationApiClient)
    {
        this.authenticationApiClient = authenticationApiClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AuthenticationView()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["HideShell"] = true;
        return View(new AuthenticationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> AuthenticationView(
        AuthenticationViewModel model,
        CancellationToken cancellationToken)
    {
        ViewData["HideShell"] = true;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            Common.Data.Entity.DTOs.AuthResponseDto response =
                await authenticationApiClient.LoginAsync(model.Username.Trim(), model.Password, cancellationToken);

            Claim[] claims =
            [
                new(ClaimTypes.Name, response.Username),
                new(ClaimTypes.Role, response.Role),
                new("access_token", response.Token)
            ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(AuthenticationView));
    }
}
