using System;
using System.Collections.Generic;
using System.Linq;
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
        {
            return erVisitRepository.GetActiveVisitsWithTriage()
                .Where(queueEntry => queueEntry.visit.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                .OrderBy(queueEntry => queueEntry.triage.Triage_Level)
                .ThenBy(queueEntry => queueEntry.visit.Arrival_date_time)
                .ToList();
        }

        public IReadOnlyList<ER_Room> GetAvailableRooms()
        {
            return roomRepository.GetAvailableRooms();
        }

        public Patient? GetPatientById(string patientId)
        {
            return patientRepository.GetById(patientId);
        }

        public Triage? GetTriageByVisitId(int visitId)
        {
            return triageRepository.GetByVisitId(visitId);
        }

        public ER_Room? FindAvailableRoom(string requiredRoomType)
        {
            return roomRepository.GetAvailableRooms()
                                  .FirstOrDefault(r => r.Room_Type == requiredRoomType);
        }

        public void AssignRoomToVisit(int visitId, int roomId)
        {
            ER_Room room = roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Available)
            {
                throw new InvalidOperationException(
                    $"Room {roomId} is not available (current: '{room.Availability_Status}').");
            }

            ER_Visit visit = erVisitRepository.GetByVisitId(visitId)
                ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

            if (visit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                throw new InvalidOperationException(
                    $"Visit {visitId} is not in WAITING_FOR_ROOM (current: '{visit.Status}').");
            }

            UpdateRoomAvailability(roomId, ER_Room.RoomStatus.Occupied);
            roomRepository.SetCurrentVisit(roomId, visitId);
            stateManagementService.ChangeVisitStatus(visitId, ER_Visit.VisitStatus.IN_ROOM);
            Logger.Info($"Visit {visitId} assigned to Room {roomId}.");
        }

        public void UpdateRoomAvailability(int roomId, string newStatus)
        {
            ER_Room room = roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            room.UpdateAvailabilityStatus(newStatus);
            roomRepository.UpdateAvailabilityStatus(roomId, newStatus);
        }

        /// <summary>
        /// Auto-assign: picks the highest-priority WAITING_FOR_ROOM visit using
        /// QueueService ordering (triage level asc, arrival asc), determines room type
        /// from triage params, finds a matching available room, and assigns it.
        /// Uses ERVisitRepository.GetActiveVisitsWithTriage() — same data QueueService uses.
        /// </summary>
        public bool AutoAssignRoom()
        {
            // Get waiting visits with triage, ordered by priority (same as QueueService)
            var waitingWithTriage = GetWaitingVisitsWithTriage();

            if (waitingWithTriage.Count == 0)
            {
                return false;
            }

            var (topVisit, topTriage) = waitingWithTriage.First();

            var parameters = triageParamsRepository.GetByTriageId(topTriage.Triage_ID);

            // Defaulting parameters to 1 if missing for safety
            int bleeding = parameters?.Bleeding ?? 1;
            int injuryType = parameters?.Injury_Type ?? 1;
            int consciousness = parameters?.Consciousness ?? 1;
            int breathing = parameters?.Breathing ?? 1;

            string requiredType = RoomTypeHelper.DetermineRoomType(
                topTriage.Specialization, bleeding, injuryType, consciousness, breathing);

            ER_Room? room = FindAvailableRoom(requiredType);
            if (room == null)
            {
                Logger.Warning($"AutoAssignRoom: No '{requiredType}' room available for Visit {topVisit.Visit_ID}.");
                return false;
            }

            AssignRoomToVisit(topVisit.Visit_ID, room.Room_ID);
            return true;
        }
    }
}
