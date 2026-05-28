using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Models.Examination;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class ExaminationController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;
    private readonly IErStaffService erStaffService;

    public ExaminationController(IErWorkflowApiClient erApiClient, IErStaffService erStaffService)
    {
        this.erApiClient = erApiClient;
        this.erStaffService = erStaffService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            ExaminationViewModel model = await BuildModelAsync(selectedVisitId, new ExaminationFormViewModel(), cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new ExaminationViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestDoctor(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            ER_Visit visit = await erApiClient.GetVisitAsync(visitId, cancellationToken)
                ?? throw new KeyNotFoundException($"Visit {visitId} was not found.");

            if (!string.Equals(visit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Doctor assignment is only available for visits currently in a room.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
            }

            Triage triage = await erApiClient.GetTriageByVisitIdAsync(visitId, cancellationToken)
                ?? throw new InvalidOperationException($"Triage was not found for visit {visitId}.");
            Triage_Parameters parameters = await erApiClient.GetTriageParametersByTriageIdAsync(triage.Triage_ID, cancellationToken)
                ?? throw new InvalidOperationException($"Triage parameters were not found for triage {triage.Triage_ID}.");

            ErDoctorAssignment doctor = erStaffService.RequestDoctor(triage.Specialization, parameters);
            await erApiClient.UpdateVisitStatusAsync(visitId, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, cancellationToken);

            TempData["SuccessMessage"] = $"Doctor {doctor.name} ({doctor.specialty}) assigned to visit {visitId}.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind(Prefix = "Form")] ExaminationFormViewModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ExaminationViewModel invalidModel = await BuildModelAsync(form.VisitId, form, cancellationToken);
            return View("Index", invalidModel);
        }

        try
        {
            ER_Visit visit = await erApiClient.GetVisitAsync(form.VisitId, cancellationToken)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            if (!string.Equals(visit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(visit.Status, ER_Visit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "The visit must be waiting for a doctor before the examination can be saved.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            int roomId = await ResolveAssignedRoomIdAsync(form.VisitId, cancellationToken);
            List<Examination> visitExaminations = await erApiClient.GetExaminationsByVisitIdAsync(form.VisitId, cancellationToken);
            Examination? existing = visitExaminations.OrderByDescending(examination => examination.Exam_Time).FirstOrDefault();

            if (existing is null)
            {
                await erApiClient.CreateExaminationAsync(new Examination
                {
                    Visit_ID = form.VisitId,
                    Doctor_ID = form.DoctorId,
                    Exam_Time = DateTime.Now,
                    Room_ID = roomId,
                    Notes = form.Notes.Trim()
                }, cancellationToken);
            }
            else
            {
                existing.Doctor_ID = form.DoctorId;
                existing.Room_ID = roomId;
                existing.Notes = form.Notes.Trim();
                await erApiClient.UpdateExaminationAsync(existing.Exam_ID, existing, cancellationToken);
            }

            await erApiClient.UpdateVisitStatusAsync(form.VisitId, ER_Visit.VisitStatus.IN_EXAMINATION, cancellationToken);
            TempData["SuccessMessage"] = $"Examination for visit {form.VisitId} was saved.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
    }

    [HttpGet]
    public async Task<IActionResult> Summary(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            ERExaminationSummaryDto summary = await erApiClient.GetExaminationSummaryAsync(visitId, cancellationToken)
                ?? throw new InvalidOperationException("No examination summary is available for this visit.");

            ErDoctorAssignment doctor = erStaffService.GetDoctorById(summary.DoctorId);
            summary.AssignedDoctorName = $"{doctor.name} ({doctor.specialty})";

            return View(new ExaminationSummaryViewModel
            {
                VisitId = visitId,
                Summary = summary
            });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }
    }

    private async Task<ExaminationViewModel> BuildModelAsync(
        int? selectedVisitId,
        ExaminationFormViewModel form,
        CancellationToken cancellationToken)
    {
        List<ER_Visit> eligibleVisits = await erApiClient.GetEligibleExaminationVisitsAsync(cancellationToken);
        ER_Visit? selectedVisit = selectedVisitId.HasValue
            ? eligibleVisits.FirstOrDefault(visit => visit.Visit_ID == selectedVisitId.Value)
                ?? await erApiClient.GetVisitAsync(selectedVisitId.Value, cancellationToken)
            : null;

        var model = new ExaminationViewModel
        {
            SelectedVisitId = selectedVisitId,
            EligibleVisits = eligibleVisits
                .OrderBy(visit => visit.Arrival_date_time)
                .Select(MapVisit)
                .ToList(),
            SelectedVisit = selectedVisit is null ? null : MapVisit(selectedVisit),
            Form = form
        };

        if (selectedVisit is null)
        {
            return model;
        }

        model.CanRequestDoctor = string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase);
        model.CanSaveExamination =
            string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase);

        List<Examination> history = await erApiClient.GetPatientExaminationHistoryAsync(selectedVisit.Patient_ID, cancellationToken);
        model.ExaminationHistory = history
            .Select(examination => new ExaminationHistoryItemViewModel
            {
                ExamId = examination.Exam_ID,
                VisitId = examination.Visit_ID,
                DoctorId = examination.Doctor_ID,
                ExamTime = examination.Exam_Time,
                RoomId = examination.Room_ID,
                Notes = examination.Notes
            })
            .ToList();

        Triage? triage = await erApiClient.GetTriageByVisitIdAsync(selectedVisit.Visit_ID, cancellationToken);
        Triage_Parameters? triageParameters = triage is null
            ? null
            : await erApiClient.GetTriageParametersByTriageIdAsync(triage.Triage_ID, cancellationToken);
        if (triage is not null && triageParameters is not null)
        {
            model.TriageDetails = new ExaminationTriageViewModel
            {
                TriageLevel = triage.Triage_Level,
                Specialization = triage.Specialization,
                NurseId = triage.Nurse_ID,
                Consciousness = triageParameters.Consciousness,
                Breathing = triageParameters.Breathing,
                Bleeding = triageParameters.Bleeding,
                InjuryType = triageParameters.Injury_Type,
                PainLevel = triageParameters.Pain_Level
            };
        }

        Examination? existing = history.FirstOrDefault(examination => examination.Visit_ID == selectedVisit.Visit_ID);
        if (existing is not null)
        {
            ErDoctorAssignment doctor = erStaffService.GetDoctorById(existing.Doctor_ID);
            model.Form = new ExaminationFormViewModel
            {
                VisitId = selectedVisit.Visit_ID,
                DoctorId = existing.Doctor_ID,
                Notes = string.IsNullOrWhiteSpace(form.Notes) ? existing.Notes : form.Notes
            };
            model.DoctorName = doctor.name;
            model.DoctorSpecialty = doctor.specialty;
        }
        else
        {
            ErDoctorAssignment? doctor = model.TriageDetails is null
                ? null
                : erStaffService.RequestDoctor(model.TriageDetails.Specialization, new Triage_Parameters
                {
                    Consciousness = model.TriageDetails.Consciousness,
                    Breathing = model.TriageDetails.Breathing,
                    Bleeding = model.TriageDetails.Bleeding,
                    Injury_Type = model.TriageDetails.InjuryType,
                    Pain_Level = model.TriageDetails.PainLevel
                });

            model.Form.VisitId = selectedVisit.Visit_ID;
            model.Form.DoctorId = form.DoctorId == 0 ? doctor?.doctorId ?? 0 : form.DoctorId;
            model.Form.Notes = form.Notes;
            model.DoctorName = doctor?.name ?? string.Empty;
            model.DoctorSpecialty = doctor?.specialty ?? string.Empty;
        }

        return model;
    }

    private async Task<int> ResolveAssignedRoomIdAsync(int visitId, CancellationToken cancellationToken)
    {
        ER_Room? currentRoom = (await erApiClient.GetRoomsAsync(cancellationToken))
            .FirstOrDefault(room => room.Current_Visit_ID == visitId);
        if (currentRoom is not null)
        {
            return currentRoom.Room_ID;
        }

        Examination? latestExam = (await erApiClient.GetExaminationsByVisitIdAsync(visitId, cancellationToken))
            .OrderByDescending(examination => examination.Exam_Time)
            .FirstOrDefault();
        if (latestExam is not null)
        {
            return latestExam.Room_ID;
        }

        ER_Room? fallbackRoom = (await erApiClient.GetRoomsAsync(cancellationToken))
            .OrderBy(room => room.Room_ID)
            .FirstOrDefault();

        return fallbackRoom?.Room_ID ?? throw new InvalidOperationException("No ER rooms are available.");
    }

    private static ExaminationVisitViewModel MapVisit(ER_Visit visit) =>
        new ()
        {
            VisitId = visit.Visit_ID,
            PatientId = visit.Patient_ID,
            ArrivalTime = visit.Arrival_date_time,
            ChiefComplaint = visit.Chief_Complaint,
            Status = visit.Status
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening examinations.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
