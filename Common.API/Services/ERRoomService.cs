using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class ERRoomService : IERRoomService
{
    private readonly IERRoomRepository repository;
    private readonly IERVisitRepository erVisitRepository;
    private readonly ITriageRepository triageRepository;
    private readonly IPatientRepository patientRepository;

    public ERRoomService(
        IERRoomRepository repository,
        IERVisitRepository erVisitRepository,
        ITriageRepository triageRepository,
        IPatientRepository patientRepository)
    {
        this.repository = repository;
        this.erVisitRepository = erVisitRepository;
        this.triageRepository = triageRepository;
        this.patientRepository = patientRepository;
    }

    public Task<List<ER_Room>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<ER_Room?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public Task<ER_Room> CreateAsync(ER_Room room) =>
        repository.CreateAsync(room);

    public Task<bool> UpdateAsync(int id, ER_Room room) =>
        repository.UpdateAsync(id, room);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);

    public async Task<List<ER_Room>> GetByStatusAsync(string status)
    {
        return (await repository.GetAllAsync())
            .Where(room => ER_Room.StatusEquals(room.Availability_Status, status))
            .ToList();
    }

    public async Task<ERRoomVisitDetailsDto?> GetVisitDetailsAsync(int roomId)
    {
        ER_Room room = await repository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"Room {roomId} was not found.");

        if (!room.Current_Visit_ID.HasValue)
        {
            return null;
        }

        ER_Visit? visit = await erVisitRepository.GetByIdAsync(room.Current_Visit_ID.Value);

        if (visit == null)
        {
            return null;
        }

        Patient? patient = (await patientRepository.SearchAsync(new PatientFilter { CNP = visit.Patient_ID }))
            .FirstOrDefault();

        Triage? triage = (await triageRepository.GetAllAsync())
            .FirstOrDefault(item => item.Visit_ID == visit.Visit_ID);

        return new ERRoomVisitDetailsDto
        {
            Visit = visit,
            Patient = patient,
            Triage = triage
        };
    }

    public async Task MarkRoomAsCleaningAsync(int roomId)
    {
        ER_Room room = await repository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"Room {roomId} was not found.");

        int? visitId = room.Current_Visit_ID;

        room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
        room.Current_Visit_ID = null;
        await repository.UpdateAsync(roomId, room);

        if (!visitId.HasValue)
        {
            return;
        }

        ER_Visit? visit = await erVisitRepository.GetByIdAsync(visitId.Value);
        if (visit != null &&
            (string.Equals(visit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(visit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase)))
        {
            visit.Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM;
            await erVisitRepository.UpdateAsync(visit.Visit_ID, visit);
        }
    }

    public async Task MarkRoomAsAvailableAsync(int roomId)
    {
        ER_Room room = await repository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"Room {roomId} was not found.");

        room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);
        await repository.UpdateAsync(roomId, room);
    }
}
