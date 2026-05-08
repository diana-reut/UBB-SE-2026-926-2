using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Common.Data.Data;
using Microsoft.EntityFrameworkCore;
using Common.Data.Models;

namespace ERManagementSystem.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly EFHospitalDbContext context;

        public RoomRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public List<ER_Room> GetAllRooms() => GetAllRoomsAsync().GetAwaiter().GetResult();

        public Task<List<ER_Room>> GetAllRoomsAsync() => context.ERRooms.AsNoTracking().ToListAsync();

        public ER_Room? GetById(int roomId) => GetByIdAsync(roomId).GetAwaiter().GetResult();

        public Task<ER_Room?> GetByIdAsync(int roomId) =>
            context.ERRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Room_ID == roomId);

        public List<ER_Room> GetAvailableRooms() => GetAvailableRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetAvailableRoomsAsync() => GetRoomsByStatusAsync(ER_Room.RoomStatus.Available);
        public List<ER_Room> GetOccupiedRooms() => GetOccupiedRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetOccupiedRoomsAsync() => GetRoomsByStatusAsync(ER_Room.RoomStatus.Occupied);
        public List<ER_Room> GetCleaningRooms() => GetCleaningRoomsAsync().GetAwaiter().GetResult();
        public Task<List<ER_Room>> GetCleaningRoomsAsync() => GetRoomsByStatusAsync(ER_Room.RoomStatus.Cleaning);

        public List<ER_Room> GetRoomsByStatus(string status) => GetRoomsByStatusAsync(status).GetAwaiter().GetResult();

        public Task<List<ER_Room>> GetRoomsByStatusAsync(string status) =>
            context.ERRooms
                .AsNoTracking()
                .Where(r => r.Availability_Status == status)
                .ToListAsync();

        public void UpdateAvailabilityStatus(int roomId, string newStatus) =>
            UpdateAvailabilityStatusAsync(roomId, newStatus).GetAwaiter().GetResult();

        public async Task UpdateAvailabilityStatusAsync(int roomId, string newStatus)
        {
            ER_Room room = await context.ERRooms.FirstAsync(r => r.Room_ID == roomId);
            room.Availability_Status = newStatus;
            await context.SaveChangesAsync();
        }

        public void SetCurrentVisit(int roomId, int visitId) =>
            SetCurrentVisitAsync(roomId, visitId).GetAwaiter().GetResult();

        public async Task SetCurrentVisitAsync(int roomId, int visitId)
        {
            ER_Room room = await context.ERRooms.FirstAsync(r => r.Room_ID == roomId);
            room.Current_Visit_ID = visitId;
            await context.SaveChangesAsync();
        }

        public void ClearCurrentVisit(int roomId) =>
            ClearCurrentVisitAsync(roomId).GetAwaiter().GetResult();

        public async Task ClearCurrentVisitAsync(int roomId)
        {
            ER_Room room = await context.ERRooms.FirstAsync(r => r.Room_ID == roomId);
            room.Current_Visit_ID = null;
            await context.SaveChangesAsync();
        }

        public int? GetRoomIdByVisitId(int visitId) => GetRoomIdByVisitIdAsync(visitId).GetAwaiter().GetResult();

        public Task<int?> GetRoomIdByVisitIdAsync(int visitId) =>
            context.Examinations
                .Where(e => e.Visit_ID == visitId)
                .OrderByDescending(e => e.Exam_Time)
                .Select(e => (int?)e.Room_ID)
                .FirstOrDefaultAsync();

        public int? GetRoomIdByCurrentVisit(int visitId) => GetRoomIdByCurrentVisitAsync(visitId).GetAwaiter().GetResult();

        public Task<int?> GetRoomIdByCurrentVisitAsync(int visitId) =>
            context.ERRooms
                .Where(r => r.Current_Visit_ID == visitId)
                .Select(r => (int?)r.Room_ID)
                .FirstOrDefaultAsync();

        public int? GetAssignedRoomIdForVisit(int visitId) => GetAssignedRoomIdForVisitAsync(visitId).GetAwaiter().GetResult();

        public async Task<int?> GetAssignedRoomIdForVisitAsync(int visitId)
        {
            int? currentRoomId = await GetRoomIdByCurrentVisitAsync(visitId);
            return currentRoomId ?? await GetRoomIdByVisitIdAsync(visitId);
        }

        public ER_Visit? GetVisitByRoomId(int roomId)
            => GetVisitByRoomIdAsync(roomId).GetAwaiter().GetResult();

        public async Task<ER_Visit?> GetVisitByRoomIdAsync(int roomId)
        {
            int? currentVisitId = await context.ERRooms
                .Where(r => r.Room_ID == roomId)
                .Select(r => r.Current_Visit_ID)
                .FirstOrDefaultAsync();

            if (currentVisitId.HasValue)
            {
                ER_Visit? visit = await context.ERVisits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Visit_ID == currentVisitId.Value &&
                        v.Status != ER_Visit.VisitStatus.TRANSFERRED &&
                        v.Status != ER_Visit.VisitStatus.CLOSED);

                if (visit is not null)
                {
                    return visit;
                }
            }

            int? fallbackVisitId = await context.Examinations
                .Where(e => e.Room_ID == roomId)
                .OrderByDescending(e => e.Exam_Time)
                .Select(e => (int?)e.Visit_ID)
                .FirstOrDefaultAsync();

            return fallbackVisitId.HasValue
                ? await context.ERVisits.AsNoTracking().FirstOrDefaultAsync(v => v.Visit_ID == fallbackVisitId.Value)
                : null;
        }
    }
}
