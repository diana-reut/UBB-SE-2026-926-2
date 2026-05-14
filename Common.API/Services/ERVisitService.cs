using Common.Data.Models;
using Common.Data.Repository;
using Common.Data.Entity;
using Common.Data.Integration;

namespace Common.API.Services;

public class ERVisitService : IERVisitService
{
    private readonly IERVisitRepository repository;
    private readonly IERRoomRepository erRoomRepository;
    private readonly ITriageRepository triageRepository;
    private readonly ITriageParametersRepository triageParametersRepository;
    private readonly ITransferLogRepository transferLogRepository;
    private readonly IPatientRepository patientRepository;

    public ERVisitService(
        IERVisitRepository repository,
        IERRoomRepository erRoomRepository,
        ITriageRepository triageRepository,
        ITriageParametersRepository triageParametersRepository,
        ITransferLogRepository transferLogRepository,
        IPatientRepository patientRepository)
    {
        this.repository = repository;
        this.erRoomRepository = erRoomRepository;
        this.triageRepository = triageRepository;
        this.triageParametersRepository = triageParametersRepository;
        this.transferLogRepository = transferLogRepository;
        this.patientRepository = patientRepository;
    }

    public Task<List<ER_Visit>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<ER_Visit?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public Task<ER_Visit> CreateAsync(ER_Visit visit) =>
        repository.CreateAsync(visit);

    public Task<bool> UpdateAsync(int id, ER_Visit visit) =>
        repository.UpdateAsync(id, visit);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);

    public async Task<bool> AutoAssignHighestPriorityRoomAsync()
    {
        IReadOnlyList<(ER_Visit visit, Triage triage)> waitingVisits = await GetWaitingVisitsWithTriageAsync();
        if (waitingVisits.Count == 0)
        {
            return false;
        }

        (ER_Visit visit, Triage triage) = waitingVisits[0];
        Triage_Parameters? parameters = (await triageParametersRepository.GetAllAsync())
            .FirstOrDefault(item => item.Triage_ID == triage.Triage_ID);

        string requiredRoomType = DetermineRoomType(
            triage.Specialization,
            parameters?.Bleeding ?? 1,
            parameters?.Injury_Type ?? 1,
            parameters?.Consciousness ?? 1,
            parameters?.Breathing ?? 1);

        ER_Room? room = (await erRoomRepository.GetAllAsync())
            .Where(item => ER_Room.StatusEquals(item.Availability_Status, ER_Room.RoomStatus.Available))
            .FirstOrDefault(item => item.Room_Type == requiredRoomType);

        if (room == null)
        {
            return false;
        }

        await AssignRoomAsync(visit.Visit_ID, room.Room_ID);
        return true;
    }

    public async Task AssignRoomAsync(int visitId, int roomId)
    {
        ER_Room room = await erRoomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"Room {roomId} was not found.");

        if (!ER_Room.StatusEquals(room.Availability_Status, ER_Room.RoomStatus.Available))
        {
            throw new InvalidOperationException($"Room {roomId} is not available (current: '{room.Availability_Status}').");
        }

        ER_Visit visit = await repository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

        EnsureVisitStatus(visit, ER_Visit.VisitStatus.WAITING_FOR_ROOM);

        room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Occupied);
        room.Current_Visit_ID = visitId;
        visit.Status = ER_Visit.VisitStatus.IN_ROOM;

        await erRoomRepository.UpdateAsync(roomId, room);
        await repository.UpdateAsync(visitId, visit);
    }

    public async Task TransferVisitAsync(int visitId)
    {
        try
        {
            await LogTransferAsync(visitId, "SUCCESS");
            await UpdateVisitStatusAsync(visitId, ER_Visit.VisitStatus.TRANSFERRED);
            await ReleaseRoomAsync(visitId);
            await MarkPatientAsTransferredAsync(visitId);
        }
        catch
        {
            await LogTransferAsync(visitId, "FAILED");
            throw;
        }
    }

    public async Task RetryTransferAsync(int visitId)
    {
        await LogTransferAsync(visitId, "RETRYING");
        await LogTransferAsync(visitId, "SUCCESS");
        await UpdateVisitStatusAsync(visitId, ER_Visit.VisitStatus.TRANSFERRED);
        await ReleaseRoomAsync(visitId);
        await MarkPatientAsTransferredAsync(visitId);
    }

    public async Task CloseVisitAsync(int visitId)
    {
        await UpdateVisitStatusAsync(visitId, ER_Visit.VisitStatus.CLOSED);
        await ReleaseRoomAsync(visitId);
    }

    private async Task<IReadOnlyList<(ER_Visit visit, Triage triage)>> GetWaitingVisitsWithTriageAsync()
    {
        List<ER_Visit> waitingVisits = (await repository.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ER_Visit.VisitStatus.WAITING_FOR_ROOM, StringComparison.OrdinalIgnoreCase))
            .ToList();
        List<Triage> triages = await triageRepository.GetAllAsync();

        return waitingVisits
            .Join(
                triages,
                visit => visit.Visit_ID,
                triage => triage.Visit_ID,
                (visit, triage) => (visit, triage))
            .OrderBy(item => item.triage.Triage_Level)
            .ThenBy(item => item.visit.Arrival_date_time)
            .ToList();
    }

    private static string DetermineRoomType(
        string? specialization,
        int bleeding,
        int injuryType,
        int consciousness,
        int breathing)
    {
        if (specialization == "General Surgery" || bleeding == 3 || injuryType == 3)
        {
            return ER_Room.RoomType.OperatingRoom;
        }

        if (consciousness == 3 || breathing == 3)
        {
            return ER_Room.RoomType.TraumaBay;
        }

        if (specialization == "Pulmonology" || breathing == 2)
        {
            return ER_Room.RoomType.RespiratoryRoom;
        }

        if (specialization == "Neurology" || consciousness == 2)
        {
            return ER_Room.RoomType.NeurologyRoom;
        }

        if (specialization == "Orthopedics" || injuryType == 2)
        {
            return ER_Room.RoomType.OrthopedicRoom;
        }

        return ER_Room.RoomType.GeneralRoom;
    }

    private async Task UpdateVisitStatusAsync(int visitId, string nextStatus)
    {
        ER_Visit visit = await repository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

        EnsureTransitionAllowed(visit.Status, nextStatus);
        visit.Status = nextStatus;
        await repository.UpdateAsync(visitId, visit);
    }

    private static void EnsureVisitStatus(ER_Visit visit, string expectedStatus)
    {
        if (!string.Equals(visit.Status, expectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Visit {visit.Visit_ID} is not in {expectedStatus} (current: '{visit.Status}').");
        }
    }

    private static void EnsureTransitionAllowed(string currentStatus, string nextStatus)
    {
        if (!ER_Visit.ValidTransitions.TryGetValue(currentStatus, out List<string>? validNextStatuses)
            || !validNextStatuses.Contains(nextStatus))
        {
            throw new InvalidOperationException(
                $"Invalid visit status transition: '{currentStatus}' -> '{nextStatus}'.");
        }
    }

    private async Task LogTransferAsync(int visitId, string status)
    {
        await transferLogRepository.CreateAsync(new Transfer_Log
        {
            Visit_ID = visitId,
            Transfer_Time = DateTime.Now,
            Target_System = "Patient Management",
            Status = status
        });
    }

    private async Task ReleaseRoomAsync(int visitId)
    {
        ER_Room? room = (await erRoomRepository.GetAllAsync())
            .FirstOrDefault(item => item.Current_Visit_ID == visitId);

        if (room == null)
        {
            return;
        }

        if (ER_Room.StatusEquals(room.Availability_Status, ER_Room.RoomStatus.Occupied))
        {
            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
        }

        room.Current_Visit_ID = null;
        await erRoomRepository.UpdateAsync(room.Room_ID, room);
    }

    private async Task MarkPatientAsTransferredAsync(int visitId)
    {
        ER_Visit visit = await repository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

        Patient? patient = (await patientRepository.SearchAsync(new PatientFilter { CNP = visit.Patient_ID }))
            .FirstOrDefault();
        if (patient == null)
        {
            return;
        }

        patient.Transferred = true;
        await patientRepository.UpdateAsync(patient);
    }
}
