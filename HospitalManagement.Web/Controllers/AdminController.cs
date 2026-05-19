using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using HospitalManagement.Web.Models.Admin;
using HospitalManagement.Web.Models.Patients;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class AdminController : Controller
{
    private readonly IPatientService _patientService;

    public AdminController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived = false,
        int? selectedId = null)
    {
        List<Patient> searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex);

        List<Patient> visiblePatients = searchResults
            .Where(p => p.IsArchived == archived)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();

        Patient? selectedPatient = null;
        if (selectedId.HasValue)
        {
            selectedPatient = visiblePatients.FirstOrDefault(p => p.Id == selectedId.Value)
                ?? await _patientService.GetByIdAsync(selectedId.Value);

            if (selectedPatient?.IsArchived != archived)
            {
                selectedPatient = null;
            }
        }

        var model = new AdminPatientsIndexViewModel
        {
            SearchQuery = searchQuery,
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex,
            ShowArchived = archived,
            SelectedPatientId = selectedId,
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient is null ? null : MapEditPatient(selectedPatient)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult CreatePatient()
    {
        return View("~/Views/Patients/Create.cshtml", new CreatePatientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(CreatePatientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Patients/Create.cshtml", model);
        }

        var patient = new Patient
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp.Trim(),
            Dob = model.Dob,
            Sex = model.Sex,
            PhoneNo = NormalizePhone(model.PhoneNo),
            EmergencyContact = NormalizePhone(model.EmergencyContact),
            IsArchived = false,
            IsDonor = false,
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
            return View("~/Views/Patients/Create.cshtml", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePatient(
        EditPatientViewModel model,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the selected patient form and try again.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived, selectedId = model.Id });
        }

        var patient = new Patient
        {
            Id = model.Id,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp,
            Dob = model.Dob,
            Dod = model.Dod,
            Sex = model.Sex,
            PhoneNo = NormalizePhone(model.PhoneNo),
            EmergencyContact = NormalizePhone(model.EmergencyContact),
            IsArchived = model.IsArchived,
            IsDonor = model.IsDonor,
            Transferred = model.Transferred
        };

        try
        {
            await _patientService.UpdatePatientAsync(patient);
            TempData["SuccessMessage"] = "Patient updated successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived, selectedId = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivePatient(
        int id,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived)
    {
        Patient? patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived });
        }

        await _patientService.ArchivePatientAsync(patient);
        TempData["SuccessMessage"] = $"Archived {patient.FullName}.";
        return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived = true, selectedId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DearchivePatient(
        int id,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived)
    {
        try
        {
            await _patientService.DearchivePatientAsync(id);
            TempData["SuccessMessage"] = "Patient restored to active records.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived = false, selectedId = id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsDeceased(
        int id,
        DateTime? deathDate,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived)
    {
        if (!deathDate.HasValue)
        {
            TempData["ErrorMessage"] = "Please choose a date of death.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived, selectedId = id });
        }

        try
        {
            await _patientService.ArchiveAsDeceasedAsync(id, deathDate.Value);
            TempData["SuccessMessage"] = "Patient marked as deceased.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived = true, selectedId = id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex, archived, selectedId = id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        Patient patient = await _patientService.GetPatientDetailsAsync(id);

        var history = patient.MedicalHistory;
        var model = new PatientDetailsViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.Dob,
            Sex = patient.Sex.ToString(),
            Cnp = patient.Cnp,
            PhoneNo = FormatPhoneNumber(patient.PhoneNo),
            EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
            IsArchived = patient.IsArchived,
            BloodType = history?.BloodType?.ToString(),
            Rh = history?.Rh?.ToString(),
            ChronicConditions = history?.ChronicConditions is { Count: > 0 }
                ? string.Join(", ", history.ChronicConditions)
                : "None",
            Allergies = await _patientService.GetPatientAllergiesAsync(id),
            MedicalRecords = history?.MedicalRecords?
                .OrderByDescending(r => r.ConsultationDate)
                .Select(r => new PatientMedicalRecordViewModel
                {
                    Id = r.Id,
                    ConsultationDate = r.ConsultationDate,
                    SourceType = r.SourceType.ToString(),
                    StaffId = r.StaffId,
                    Symptoms = r.Symptoms ?? "N/A",
                    Diagnosis = r.Diagnosis ?? "N/A"
                })
                .ToList() ?? []
        };

        return View(model);
    }

    private async Task<List<Patient>> SearchPatientsAsync(string? searchQuery, int? minAge, int? maxAge, Sex? sex)
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

        return await _patientService.SearchPatientsAsync(filter);
    }

    private static PatientListItemViewModel MapPatientListItem(Patient patient)
    {
        return new PatientListItemViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Cnp = patient.Cnp,
            Dob = patient.Dob,
            Sex = patient.Sex.ToString(),
            PhoneNo = FormatPhoneNumber(patient.PhoneNo),
            EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
            IsArchived = patient.IsArchived,
            IsDeceased = patient.IsDeceased
        };
    }

    private static EditPatientViewModel MapEditPatient(Patient patient)
    {
        return new EditPatientViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Cnp = patient.Cnp,
            Dob = patient.Dob,
            Dod = patient.Dod,
            Sex = patient.Sex,
            PhoneNo = FormatPhoneNumber(patient.PhoneNo),
            EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
            IsArchived = patient.IsArchived,
            IsDonor = patient.IsDonor,
            Transferred = patient.Transferred
        };
    }

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (normalized.StartsWith("+40", StringComparison.Ordinal))
        {
            return $"0{normalized[3..]}";
        }

        return normalized;
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = NormalizePhone(phone);
        if (!normalized.StartsWith('0') || normalized.Length != 10)
        {
            return phone;
        }

        return $"+40 {normalized.Substring(1, 3)} {normalized.Substring(4, 3)} {normalized.Substring(7, 3)}";
    }
}
