using System.Security.Claims;
using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> AuthenticationView()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(WebSessionKeys.AccessToken)))
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Your session expired. Please sign in again.";
                return RedirectToAction(nameof(AuthenticationView));
            }

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

            HttpContext.Session.SetString(WebSessionKeys.AccessToken, response.Token);
            HttpContext.Session.SetString(WebSessionKeys.Username, response.Username);
            HttpContext.Session.SetString(WebSessionKeys.Role, response.Role);

            Claim[] claims =
            [
                new (ClaimTypes.Name, response.Username),
                new (ClaimTypes.Role, response.Role)
            ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            TempData["LoginMessage"] = $"Signed in as {response.Username}.";
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
        HttpContext.Session.Remove(WebSessionKeys.AccessToken);
        HttpContext.Session.Remove(WebSessionKeys.Username);
        HttpContext.Session.Remove(WebSessionKeys.Role);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(AuthenticationView));
    }
}
