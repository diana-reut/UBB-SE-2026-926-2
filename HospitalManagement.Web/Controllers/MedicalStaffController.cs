using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.MedicalStaff;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class MedicalStaffController : Controller
{
    private readonly IPatientApiClient _patientApiClient;

    public MedicalStaffController(IPatientApiClient patientApiClient)
    {
        _patientApiClient = patientApiClient;
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
            HasSearched = searchQuery is not null,
            SelectedPatientId = selectedId
        };

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            try
            {
                string trimmed = searchQuery.Trim();
                var dto = trimmed.Length == 13 && trimmed.All(char.IsDigit)
                    ? new SearchPatientsDto { Cnp = trimmed }
                    : new SearchPatientsDto { NamePart = trimmed };

                List<Patient> results = await _patientApiClient.SearchPatientsAsync(dto, cancellationToken);

                if (results.Count == 0)
                    model.ErrorMessage = "There are no patients with this name or CNP.";
                else
                    model.SearchResults = results.Select(p => new PatientSearchResultViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Cnp = p.Cnp,
                        Dob = p.Dob
                    }).ToList();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Database connection error: " + ex.Message;
            }
        }

        return View(model);
    }
}