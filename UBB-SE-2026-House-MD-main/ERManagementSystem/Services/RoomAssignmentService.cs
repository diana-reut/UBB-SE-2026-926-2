using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class RoomAssignmentService : IRoomAssignmentService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IERVisitRepository erVisitRepository;
        private readonly IStateManagementService stateManagementService;
        private readonly ITriageParametersRepository triageParamsRepository;
        private readonly IPatientRepository patientRepository;
        private readonly ITriageRepository triageRepository;

        public RoomAssignmentService(
            IRoomRepository roomRepository,
            IERVisitRepository erVisitRepository,
            IStateManagementService stateManagementService,
            ITriageParametersRepository triageParamsRepository,
            IPatientRepository patientRepository,
            ITriageRepository triageRepository)
        {
            this.roomRepository = roomRepository;
            this.erVisitRepository = erVisitRepository;
            this.stateManagementService = stateManagementService;
            this.triageParamsRepository = triageParamsRepository;
            this.patientRepository = patientRepository;
            this.triageRepository = triageRepository;
        }

        public IReadOnlyList<(ER_Visit visit, Triage triage)> GetWaitingVisitsWithTriage()
            => GetWaitingVisitsWithTriageAsync().GetAwaiter().GetResult();

        public async Task<IReadOnlyList<(ER_Visit visit, Triage triage)>> GetWaitingVisitsWithTriageAsync()
        {
            return (await erVisitRepository.GetActiveVisitsWithTriageAsync())
                .Where(queueEntry => queueEntry.visit.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                .OrderBy(queueEntry => queueEntry.triage.Triage_Level)
                .ThenBy(queueEntry => queueEntry.visit.Arrival_date_time)
                .ToList();
        }

        public IReadOnlyList<ER_Room> GetAvailableRooms()
            => GetAvailableRoomsAsync().GetAwaiter().GetResult();

        public async Task<IReadOnlyList<ER_Room>> GetAvailableRoomsAsync()
            => await roomRepository.GetAvailableRoomsAsync();

        public Patient? GetPatientById(string patientId)
            => GetPatientByIdAsync(patientId).GetAwaiter().GetResult();

        public Task<Patient?> GetPatientByIdAsync(string patientId)
            => patientRepository.GetByIdAsync(patientId);

        public Triage? GetTriageByVisitId(int visitId)
            => GetTriageByVisitIdAsync(visitId).GetAwaiter().GetResult();

        public Task<Triage?> GetTriageByVisitIdAsync(int visitId)
            => triageRepository.GetByVisitIdAsync(visitId);

        public ER_Room? FindAvailableRoom(string requiredRoomType)
            => FindAvailableRoomAsync(requiredRoomType).GetAwaiter().GetResult();

        public async Task<ER_Room?> FindAvailableRoomAsync(string requiredRoomType)
        {
            IReadOnlyList<ER_Room> rooms = await GetAvailableRoomsAsync();
            return rooms.FirstOrDefault(r => r.Room_Type == requiredRoomType);
        }

        public void AssignRoomToVisit(int visitId, int roomId)
            => AssignRoomToVisitAsync(visitId, roomId).GetAwaiter().GetResult();

        public async Task AssignRoomToVisitAsync(int visitId, int roomId)
        {
            ER_Room room = await roomRepository.GetByIdAsync(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (!ER_Room.StatusEquals(room.Availability_Status, ER_Room.RoomStatus.Available))
            {
                throw new InvalidOperationException(
                    $"Room {roomId} is not available (current: '{room.Availability_Status}').");
            }

            ER_Visit visit = await erVisitRepository.GetByVisitIdAsync(visitId)
                ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

            if (visit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                throw new InvalidOperationException(
                    $"Visit {visitId} is not in WAITING_FOR_ROOM (current: '{visit.Status}').");
            }

            await UpdateRoomAvailabilityAsync(roomId, ER_Room.RoomStatus.Occupied);
            await roomRepository.SetCurrentVisitAsync(roomId, visitId);
            await stateManagementService.ChangeVisitStatusAsync(visitId, ER_Visit.VisitStatus.IN_ROOM);
            Logger.Info($"Visit {visitId} assigned to Room {roomId}.");
        }

        public void UpdateRoomAvailability(int roomId, string newStatus)
            => UpdateRoomAvailabilityAsync(roomId, newStatus).GetAwaiter().GetResult();

        public async Task UpdateRoomAvailabilityAsync(int roomId, string newStatus)
        {
            ER_Room room = await roomRepository.GetByIdAsync(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            room.UpdateAvailabilityStatus(newStatus);
            await roomRepository.UpdateAvailabilityStatusAsync(roomId, newStatus);
        }

        /// <summary>
        /// Auto-assign: picks the highest-priority WAITING_FOR_ROOM visit using
        /// QueueService ordering (triage level asc, arrival asc), determines room type
        /// from triage params, finds a matching available room, and assigns it.
        /// Uses ERVisitRepository.GetActiveVisitsWithTriage() — same data QueueService uses.
        /// </summary>
        public bool AutoAssignRoom()
            => AutoAssignRoomAsync().GetAwaiter().GetResult();

        public async Task<bool> AutoAssignRoomAsync()
        {
            // Get waiting visits with triage, ordered by priority (same as QueueService)
            IReadOnlyList<(ER_Visit visit, Triage triage)> waitingWithTriage = await GetWaitingVisitsWithTriageAsync();

            if (waitingWithTriage.Count == 0)
            {
                return false;
            }

            var (topVisit, topTriage) = waitingWithTriage.First();

            Triage_Parameters? parameters = await triageParamsRepository.GetByTriageIdAsync(topTriage.Triage_ID);

            // Defaulting parameters to 1 if missing for safety
            int bleeding = parameters?.Bleeding ?? 1;
            int injuryType = parameters?.Injury_Type ?? 1;
            int consciousness = parameters?.Consciousness ?? 1;
            int breathing = parameters?.Breathing ?? 1;

            string requiredType = RoomTypeHelper.DetermineRoomType(
                topTriage.Specialization, bleeding, injuryType, consciousness, breathing);

            ER_Room? room = await FindAvailableRoomAsync(requiredType);
            if (room == null)
            {
                Logger.Warning($"AutoAssignRoom: No '{requiredType}' room available for Visit {topVisit.Visit_ID}.");
                return false;
            }

            await AssignRoomToVisitAsync(topVisit.Visit_ID, room.Room_ID);
            return true;
        }
    }
}
