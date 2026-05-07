using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class RoomManagementService : IRoomManagementService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IPatientRepository patientRepository;
        private readonly ITriageRepository triageRepository;

        public RoomManagementService(
            IRoomRepository roomRepository,
            IPatientRepository patientRepository,
            ITriageRepository triageRepository)
        {
            this.roomRepository = roomRepository;
            this.patientRepository = patientRepository;
            this.triageRepository = triageRepository;
        }

        public List<ER_Room> GetAvailableRooms() => GetAvailableRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetAvailableRoomsAsync() => roomRepository.GetAvailableRoomsAsync();
        public List<ER_Room> GetOccupiedRooms() => GetOccupiedRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetOccupiedRoomsAsync() => roomRepository.GetOccupiedRoomsAsync();
        public List<ER_Room> GetCleaningRooms() => GetCleaningRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetCleaningRoomsAsync() => roomRepository.GetCleaningRoomsAsync();

        public void MarkRoomAsCleaning(int roomId)
            => MarkRoomAsCleaningAsync(roomId).GetAwaiter().GetResult();

        public async Task MarkRoomAsCleaningAsync(int roomId)
        {
            ER_Room room = await roomRepository.GetByIdAsync(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Occupied)
            {
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be set to cleaning from '{room.Availability_Status}'. Must be 'occupied'.");
            }

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
            await roomRepository.UpdateAvailabilityStatusAsync(roomId, ER_Room.RoomStatus.Cleaning);
            await roomRepository.ClearCurrentVisitAsync(roomId);   // clear visit link so panel doesn't show stale data
            Logger.Info($"Room {roomId} set to cleaning.");
        }

        public void MarkRoomAsCleaned(int roomId)
            => MarkRoomAsCleanedAsync(roomId).GetAwaiter().GetResult();

        public async Task MarkRoomAsCleanedAsync(int roomId)
        {
            ER_Room room = await roomRepository.GetByIdAsync(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Cleaning)
            {
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be marked as cleaned — current status is '{room.Availability_Status}', not 'cleaning'.");
            }

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);
            await roomRepository.UpdateAvailabilityStatusAsync(roomId, ER_Room.RoomStatus.Available);
            Logger.Info($"Room {roomId} is now available.");
        }

        public RoomVisitDetails? GetRoomVisitDetails(int roomId)
            => GetRoomVisitDetailsAsync(roomId).GetAwaiter().GetResult();

        public async Task<RoomVisitDetails?> GetRoomVisitDetailsAsync(int roomId)
        {
            ER_Visit? visit = await roomRepository.GetVisitByRoomIdAsync(roomId);
            if (visit == null)
            {
                return null;
            }

            return new RoomVisitDetails
            {
                Visit = visit,
                Patient = await patientRepository.GetByIdAsync(visit.Patient_ID),
                Triage = await triageRepository.GetByVisitIdAsync(visit.Visit_ID)
            };
        }
    }
}
