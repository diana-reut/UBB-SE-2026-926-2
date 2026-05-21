using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Models.RoomAssignment;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class RoomAssignmentController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;
    private readonly IPatientApiClient patientApiClient;

    public RoomAssignmentController(IErWorkflowApiClient erApiClient, IPatientApiClient patientApiClient)
    {
        this.erApiClient = erApiClient;
        this.patientApiClient = patientApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            RoomAssignmentViewModel model = await BuildModelAsync(selectedVisitId, null, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new RoomAssignmentViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoAssign(CancellationToken cancellationToken)
    {
        try
        {
            bool assigned = await erApiClient.AutoAssignHighestPriorityRoomAsync(cancellationToken);
            TempData[assigned ? "SuccessMessage" : "ErrorMessage"] = assigned
                ? "The highest-priority visit was assigned to a matching room."
                : "No suitable room is currently available for the highest-priority visit.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int visitId, int roomId, CancellationToken cancellationToken)
    {
        if (visitId <= 0 || roomId <= 0)
        {
            TempData["ErrorMessage"] = "Select both a waiting visit and an available room.";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        try
        {
            await erApiClient.AssignRoomAsync(visitId, roomId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} was assigned to room {roomId}.";
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

        return RedirectToAction(nameof(Index));
    }

    private async Task<RoomAssignmentViewModel> BuildModelAsync(
        int? selectedVisitId,
        int? selectedRoomId,
        CancellationToken cancellationToken)
    {
        List<ER_Visit> waitingVisits = await erApiClient.GetVisitsByStatusAsync(
            ER_Visit.VisitStatus.WAITING_FOR_ROOM,
            cancellationToken);
        List<Triage> triages = await erApiClient.GetTriagesAsync(cancellationToken);
        List<Triage_Parameters> triageParameters = await erApiClient.GetTriageParametersAsync(cancellationToken);
        HashSet<int> triageIdsWithParameters = triageParameters
            .Select(parameters => parameters.Triage_ID)
            .ToHashSet();
        List<ER_Room> availableRooms = await erApiClient.GetRoomsByStatusAsync(
            ER_Room.RoomStatus.Available,
            cancellationToken);

        var model = new RoomAssignmentViewModel
        {
            SelectedVisitId = selectedVisitId,
            SelectedRoomId = selectedRoomId,
            WaitingVisits = waitingVisits
                .Select(visit =>
                {
                    Triage? triage = triages.FirstOrDefault(item => item.Visit_ID == visit.Visit_ID);
                    bool hasTriageData = triage is not null && triageIdsWithParameters.Contains(triage.Triage_ID);

                    return new RoomAssignmentVisitViewModel
                    {
                        VisitId = visit.Visit_ID,
                        PatientId = visit.Patient_ID,
                        ArrivalTime = visit.Arrival_date_time,
                        ChiefComplaint = visit.Chief_Complaint,
                        Status = visit.Status,
                        TriageLevel = triage?.Triage_Level,
                        Specialization = triage?.Specialization,
                        HasTriageData = hasTriageData,
                        WarningMessage = hasTriageData
                            ? null
                            : triage is null
                                ? "Triage record is missing."
                                : "Triage parameters are missing."
                    };
                })
                .OrderBy(item => item.TriageLevel ?? int.MaxValue)
                .ThenBy(item => item.ArrivalTime)
                .ToList(),
            AvailableRooms = availableRooms
                .OrderBy(room => room.Room_ID)
                .Select(room => new RoomOptionViewModel
                {
                    RoomId = room.Room_ID,
                    RoomType = room.Room_Type,
                    Status = room.Availability_Status
                })
                .ToList()
        };

        if (!selectedVisitId.HasValue)
        {
            return model;
        }

        ER_Visit? selectedVisit = waitingVisits.FirstOrDefault(visit => visit.Visit_ID == selectedVisitId.Value)
            ?? await erApiClient.GetVisitAsync(selectedVisitId.Value, cancellationToken);
        if (selectedVisit is null)
        {
            return model;
        }

        Patient? patient = (await patientApiClient.SearchPatientsAsync(
            new SearchPatientsDto { Cnp = selectedVisit.Patient_ID },
            cancellationToken)).FirstOrDefault();

        model.SelectedPatient = new RoomAssignmentPatientViewModel
        {
            PatientId = selectedVisit.Patient_ID,
            Name = patient?.FullName ?? selectedVisit.Patient_ID,
            Phone = patient?.PhoneNo ?? string.Empty
        };

        Triage? selectedTriage = triages.FirstOrDefault(triage => triage.Visit_ID == selectedVisit.Visit_ID);
        bool selectedVisitHasParameters = selectedTriage is not null && triageIdsWithParameters.Contains(selectedTriage.Triage_ID);
        if (selectedTriage is not null)
        {
            model.SelectedTriage = new RoomAssignmentTriageViewModel
            {
                TriageLevel = selectedTriage.Triage_Level,
                Specialization = selectedTriage.Specialization,
                NurseId = selectedTriage.Nurse_ID
            };
        }

        if (!selectedVisitHasParameters)
        {
            model.ErrorMessage = selectedTriage is null
                ? "The selected visit cannot be assigned to a room because its triage record is missing."
                : "The selected visit cannot be assigned to a room because its triage parameters are missing.";
        }

        return model;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening room assignment.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
