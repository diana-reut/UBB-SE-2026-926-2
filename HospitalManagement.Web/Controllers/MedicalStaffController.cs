using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Web.Models.MedicalStaff;
using HospitalManagement.Web.Models.Patients;
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

    [HttpGet]
    public async Task<IActionResult> PatientProfile(int id, int? selectedRecordId, CancellationToken cancellationToken = default)
    {
        try
        {
            Patient patient = await _patientApiClient.GetPatientDetailsAsync(id, cancellationToken);
            List<string> allergies = await _patientApiClient.GetPatientAllergiesAsync(id, cancellationToken);
            bool isHighRisk = await _patientApiClient.IsHighRiskAsync(id, cancellationToken);

            var model = new PatientProfileViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Cnp = patient.Cnp,
                BloodType = patient.MedicalHistory?.BloodType?.ToString() ?? "N/A",
                Rh = patient.MedicalHistory?.Rh?.ToString() ?? "N/A",
                FormattedAllergies = allergies.Count > 0 ? string.Join(", ", allergies) : "None",
                FormattedChronicConditions = patient.MedicalHistory?.ChronicConditions is { Count: > 0 }
                    ? string.Join(", ", patient.MedicalHistory.ChronicConditions)
                    : "None",
                IsHighRisk = isHighRisk,
                MedicalRecords = patient.MedicalHistory?.MedicalRecords?
                    .OrderByDescending(r => r.ConsultationDate)
                    .Select(r => new MedicalRecordViewModel
                    {
                        Id = r.Id,
                        ConsultationDate = r.ConsultationDate,
                        SourceType = r.SourceType.ToString(),
                        StaffId = r.StaffId,
                        Symptoms = r.Symptoms ?? "N/A",
                        Diagnosis = r.Diagnosis ?? "N/A"
                    }).ToList() ?? []
            };
            model.SelectedRecordId = selectedRecordId;
            return View("~/Views/Patients/PatientProfile.cshtml", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load patient profile: " + ex.Message;
            return RedirectToAction(nameof(Dashboard));
        }
    }
}