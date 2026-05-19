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
        return RedirectToAction("Index", "Admin", new { searchQuery, minAge, maxAge, sex });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return RedirectToAction("CreatePatient", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientViewModel model)
    {
        return RedirectToAction("CreatePatient", "Admin");
    }
}
