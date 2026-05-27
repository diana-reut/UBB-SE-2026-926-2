using Common.Data.Entity.DTOs;
using Common.Data.Models;
using HospitalManagement.Web.Models.RoomManagement;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class RoomManagementController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public RoomManagementController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedRoomId, CancellationToken cancellationToken)
    {
        try
        {
            RoomManagementViewModel model = await BuildModelAsync(selectedRoomId, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new RoomManagementViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCleaning(int roomId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.MarkRoomAsCleaningAsync(roomId, cancellationToken);
            TempData["SuccessMessage"] = $"Room {roomId} is now marked for cleaning.";
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
    public async Task<IActionResult> MarkAvailable(int roomId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.MarkRoomAsAvailableAsync(roomId, cancellationToken);
            TempData["SuccessMessage"] = $"Room {roomId} is available again.";
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

    private async Task<RoomManagementViewModel> BuildModelAsync(int? selectedRoomId, CancellationToken cancellationToken)
    {
        List<ER_Room> availableRooms = await erApiClient.GetRoomsByStatusAsync(ER_Room.RoomStatus.Available, cancellationToken);
        List<ER_Room> occupiedRooms = await erApiClient.GetRoomsByStatusAsync(ER_Room.RoomStatus.Occupied, cancellationToken);
        List<ER_Room> cleaningRooms = await erApiClient.GetRoomsByStatusAsync(ER_Room.RoomStatus.Cleaning, cancellationToken);

        var model = new RoomManagementViewModel
        {
            SelectedRoomId = selectedRoomId,
            AvailableRooms = availableRooms.Select(MapRoom).ToList(),
            OccupiedRooms = occupiedRooms.Select(MapRoom).ToList(),
            CleaningRooms = cleaningRooms.Select(MapRoom).ToList()
        };

        if (!selectedRoomId.HasValue)
        {
            return model;
        }

        ERRoomVisitDetailsDto? visitDetails = await erApiClient.GetRoomVisitDetailsAsync(selectedRoomId.Value, cancellationToken);
        if (visitDetails?.Visit is null)
        {
            return model;
        }

        model.SelectedRoomVisit = new RoomVisitDetailsViewModel
        {
            VisitId = visitDetails.Visit.Visit_ID,
            PatientId = visitDetails.Visit.Patient_ID,
            PatientName = visitDetails.Patient?.FullName ?? visitDetails.Visit.Patient_ID,
            ChiefComplaint = visitDetails.Visit.Chief_Complaint,
            VisitStatus = visitDetails.Visit.Status,
            TriageLevel = visitDetails.Triage?.Triage_Level,
            Specialization = visitDetails.Triage?.Specialization
        };

        return model;
    }

    private static RoomStatusItemViewModel MapRoom(ER_Room room) =>
        new ()
        {
            RoomId = room.Room_ID,
            RoomType = room.Room_Type,
            Status = room.Availability_Status,
            CurrentVisitId = room.Current_Visit_ID
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening room management.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
