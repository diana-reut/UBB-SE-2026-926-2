using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Integration;
using HospitalManagement.Web.Models.Patients;
using Microsoft.AspNetCore.Mvc;
using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Controllers;

public class PatientsController : Controller
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery, int? minAge, int? maxAge, Sex? sex)
    {
        var filter = new PatientFilter
        {
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex
        };

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string trimmedQuery = searchQuery.Trim();
            if (trimmedQuery.All(char.IsDigit) && trimmedQuery.Length == 13)
            {
                filter.CNP = trimmedQuery;
            }
            else
            {
                filter.NamePart = trimmedQuery;
            }
        }

        List<Patient> patients = await _patientService.SearchPatientsAsync(filter);

        var model = new PatientsIndexViewModel
        {
            SearchQuery = searchQuery,
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex,
            Patients = patients
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Select(p => new PatientListItemViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Cnp = p.Cnp,
                    Dob = p.Dob,
                    Sex = p.Sex.ToString(),
                    PhoneNo = FormatPhoneNumber(p.PhoneNo),
                    EmergencyContact = FormatPhoneNumber(p.EmergencyContact),
                    IsArchived = p.IsArchived
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreatePatientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var patient = new Patient
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp.Trim(),
            Dob = model.Dob,
            Sex = model.Sex,
            PhoneNo = model.PhoneNo.Trim(),
            EmergencyContact = model.EmergencyContact.Trim(),
            IsDonor = false,
            IsArchived = false,
            Transferred = false
        };

        try
        {
            await _patientService.CreatePatientAsync(patient);
            TempData["SuccessMessage"] = $"Patient {patient.FullName} was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (!normalized.StartsWith('0') || normalized.Length != 10)
        {
            return phone;
        }

        return $"+40 {normalized.Substring(1, 3)} {normalized.Substring(4, 3)} {normalized.Substring(7, 3)}";
    }
}
