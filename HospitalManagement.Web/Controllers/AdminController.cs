using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using HospitalManagement.Web.Models.Admin;
using HospitalManagement.Web.Models.Patients;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IPatientApiClient patientApiClient;
    private readonly IAllergyApiClient allergyApiClient;

    public AdminController(IPatientApiClient patientApiClient, IAllergyApiClient allergyApiClient)
    {
        this.patientApiClient = patientApiClient;
        this.allergyApiClient = allergyApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived = false,
        int? selectedId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Patient> searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex, cancellationToken);

            List<Patient> visiblePatients = searchResults
                .Where(p => p.IsArchived == archived)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            Patient? selectedPatient = null;
            if (selectedId.HasValue)
            {
                selectedPatient = visiblePatients.FirstOrDefault(p => p.Id == selectedId.Value)
                    ?? await patientApiClient.GetByIdAsync(selectedId.Value, cancellationToken);

                if (selectedPatient?.IsArchived != archived)
                {
                    selectedPatient = null;
                }
            }

            List<PatientListItemViewModel> patientRows = visiblePatients.Select(MapPatientListItem).ToList();
            PatientListItemViewModel? selectedPatientRow = selectedPatient is null
                ? null
                : patientRows.FirstOrDefault(p => p.Id == selectedPatient.Id);

            EditPatientViewModel? selectedPatientModel = selectedPatient is null
                ? null
                : MapEditPatient(selectedPatient, selectedPatientRow);

            var model = new AdminPatientsIndexViewModel
            {
                SearchQuery = searchQuery,
                MinAge = minAge,
                MaxAge = maxAge,
                Sex = sex,
                ShowArchived = archived,
                SelectedPatientId = selectedPatient?.Id,
                Patients = patientRows,
                SelectedPatient = selectedPatientModel
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(new AdminPatientsIndexViewModel
            {
                SearchQuery = searchQuery,
                MinAge = minAge,
                MaxAge = maxAge,
                Sex = sex,
                ShowArchived = archived
            });
        }
    }

    [HttpGet]
    public IActionResult CreatePatient()
    {
        return View("~/Views/Patients/Create.cshtml", new CreatePatientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(
        CreatePatientViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Patients/Create.cshtml", model);
        }

        var dto = new CreatePatientDto
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp.Trim(),
            Dob = model.Dob,
            Sex = model.Sex,
            PhoneNo = NormalizePhone(model.PhoneNo),
            EmergencyContact = NormalizePhone(model.EmergencyContact),
            IsDonor = false
        };

        try
        {
            Patient created = await patientApiClient.CreatePatientAsync(dto, cancellationToken);
            TempData["SuccessMessage"] = $"Patient {created.FullName} was created successfully.";
            return RedirectToAction(nameof(CreateMedicalHistory), new { patientId = created.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/Patients/Create.cshtml", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateMedicalHistory(
        int patientId,
        CancellationToken cancellationToken)
    {
        try
        {
            Patient? patient = await patientApiClient.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            CreateMedicalHistoryViewModel model = await BuildMedicalHistoryModelAsync(patient, null, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMedicalHistory(
        CreateMedicalHistoryViewModel model,
        CancellationToken cancellationToken)
    {
        Patient? patient;
        try
        {
            patient = await patientApiClient.GetByIdAsync(model.PatientId, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }

        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildMedicalHistoryModelAsync(patient, model, cancellationToken));
        }

        var dto = new CreateMedicalHistoryDto
        {
            BloodType = model.BloodType,
            Rh = model.Rh,
            ChronicConditions = SplitConditions(model.ChronicConditionsText),
            AllergyIds = model.AllergyIds.Distinct().ToList()
        };

        try
        {
            await patientApiClient.CreateMedicalHistoryAsync(model.PatientId, dto, cancellationToken);
            TempData["SuccessMessage"] = "Patient and medical history saved successfully.";
            return RedirectToAction(nameof(Index), new { selectedId = model.PatientId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildMedicalHistoryModelAsync(patient, model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SkipMedicalHistory(int patientId)
    {
        TempData["SuccessMessage"] = "Patient added successfully.";
        return RedirectToAction(nameof(Index), new { selectedId = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePatient(
        EditPatientViewModel model,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? filterSex,
        bool archived,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the selected patient form and try again.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = model.Id });
        }

        var dto = new UpdatePatientDto
        {
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
            await patientApiClient.UpdatePatientAsync(model.Id, dto, cancellationToken);
            TempData["SuccessMessage"] = "Patient updated successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivePatient(
        int id,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? filterSex,
        bool archived,
        CancellationToken cancellationToken)
    {
        try
        {
            Patient? patient = await patientApiClient.GetByIdAsync(id, cancellationToken);
            if (patient is null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived });
            }

            await patientApiClient.ArchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = $"Archived {patient.FullName}.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived = true, selectedId = id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DearchivePatient(
        int id,
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? filterSex,
        bool archived,
        CancellationToken cancellationToken)
    {
        try
        {
            await patientApiClient.DearchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Patient restored to active records.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived = false, selectedId = id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived });
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
        Sex? filterSex,
        bool archived,
        CancellationToken cancellationToken)
    {
        if (!deathDate.HasValue)
        {
            TempData["ErrorMessage"] = "Please choose a date of death.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = id });
        }

        try
        {
            await patientApiClient.ArchiveAsDeceasedAsync(id, new ArchiveAsDeceasedDto { DeathDate = deathDate.Value }, cancellationToken);
            TempData["SuccessMessage"] = "Patient marked as deceased.";
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived = true, selectedId = id });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = id });
        }
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        return RedirectToAction("Details", "Patient", new { id });
    }

    private async Task<List<Patient>> SearchPatientsAsync(
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        CancellationToken cancellationToken)
    {
        var dto = new SearchPatientsDto
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
                dto.Cnp = trimmedQuery;
            }
            else
            {
                dto.NamePart = trimmedQuery;
            }
        }

        return await patientApiClient.SearchPatientsAsync(dto, cancellationToken);
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

    private static EditPatientViewModel MapEditPatient(Patient patient, PatientListItemViewModel? patientRow)
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
            PhoneNo = CompactPhoneNumber(patient.PhoneNo),
            EmergencyContact = CompactEmergencyContact(patient.EmergencyContact),
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

    private static string CompactPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = NormalizePhone(phone);
        if (normalized.StartsWith('0') && normalized.Length == 10)
        {
            return $"+40{normalized[1..]}";
        }

        return normalized;
    }

    private static string CompactEmergencyContact(string contact)
    {
        if (string.IsNullOrWhiteSpace(contact))
        {
            return contact;
        }

        string[] parts = contact.Split(',', StringSplitOptions.None);

        return string.Join(",",
            parts.Select(part =>
            {
                string trimmed = part.Trim();
                return trimmed.Any(char.IsDigit) ? CompactPhoneNumber(trimmed) : trimmed;
            }));
    }

    private async Task<CreateMedicalHistoryViewModel> BuildMedicalHistoryModelAsync(
        Patient patient,
        CreateMedicalHistoryViewModel? source,
        CancellationToken cancellationToken)
    {
        List<AllergyOptionViewModel> allergies = (await allergyApiClient.GetAllergiesAsync(cancellationToken))
            .OrderBy(a => a.AllergyName)
            .Select(a => new AllergyOptionViewModel
            {
                Id = a.Id,
                Name = a.AllergyName
            })
            .ToList();

        return new CreateMedicalHistoryViewModel
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            BloodType = source?.BloodType ?? BloodType.A,
            Rh = source?.Rh ?? Rh.Positive,
            ChronicConditionsText = source?.ChronicConditionsText ?? string.Empty,
            AllergyIds = source?.AllergyIds ?? new List<int>(),
            AvailableAllergies = allergies
        };
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening patient administration.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }

    private static List<string> SplitConditions(string? conditionsText)
    {
        if (string.IsNullOrWhiteSpace(conditionsText))
        {
            return new List<string>();
        }

        return conditionsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
    }
}
