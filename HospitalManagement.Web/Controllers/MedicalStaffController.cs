using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.MedicalStaff;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class MedicalStaffController : Controller
{
    private readonly IPatientApiClient patientApiClient;

    public MedicalStaffController(IPatientApiClient patientApiClient)
    {
        this.patientApiClient = patientApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(
        string? searchQuery,
        int? selectedId,
        CancellationToken cancellationToken = default)
    {
        var model = new MedicalStaffDashboardViewModel
        {
            SearchQuery = searchQuery,
            HasSearched = true,
            SelectedPatientId = selectedId
        };

        try
        {
            SearchPatientsDto dto = BuildSearchDto(searchQuery);
            List<Patient> results = await patientApiClient.SearchPatientsAsync(dto, cancellationToken);

            if (results.Count == 0)
            {
                model.ErrorMessage = string.IsNullOrWhiteSpace(searchQuery)
                    ? "There are no patients."
                    : "There are no patients with this name or CNP.";
            }
            else
            {
                model.SearchResults = results.Select(p => new PatientSearchResultViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Cnp = p.Cnp,
                    Dob = p.Dob
                }).ToList();
            }
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            model.ErrorMessage = "Database connection error: " + ex.Message;
        }

        return View(model);
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening the medical staff dashboard.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }

    private static SearchPatientsDto BuildSearchDto(string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return new SearchPatientsDto();
        }

        string trimmed = searchQuery.Trim();
        return trimmed.Length == 13 && trimmed.All(char.IsDigit)
            ? new SearchPatientsDto { Cnp = trimmed }
            : new SearchPatientsDto { NamePart = trimmed };
    }
}
