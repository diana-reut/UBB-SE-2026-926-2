using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ERManagementSystem.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly EFHospitalDbContext context;

        public RoomRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public List<ER_Room> GetAllRooms() => context.ERRooms.AsNoTracking().ToList();

        public ER_Room? GetById(int roomId) =>
            context.ERRooms.AsNoTracking().FirstOrDefault(r => r.Room_ID == roomId);

        public List<ER_Room> GetAvailableRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Available);
        public List<ER_Room> GetOccupiedRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Occupied);
        public List<ER_Room> GetCleaningRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Cleaning);

        public List<ER_Room> GetRoomsByStatus(string status) =>
            context.ERRooms
                .AsNoTracking()
                .Where(r => r.Availability_Status == status)
                .ToList();

        public void UpdateAvailabilityStatus(int roomId, string newStatus)
        {
            ER_Room room = context.ERRooms.First(r => r.Room_ID == roomId);
            room.Availability_Status = newStatus;
            context.SaveChanges();
        }

        public void SetCurrentVisit(int roomId, int visitId)
        {
            ER_Room room = context.ERRooms.First(r => r.Room_ID == roomId);
            room.Current_Visit_ID = visitId;
            context.SaveChanges();
        }

        public void ClearCurrentVisit(int roomId)
        {
            ER_Room room = context.ERRooms.First(r => r.Room_ID == roomId);
            room.Current_Visit_ID = null;
            context.SaveChanges();
        }

        public int? GetRoomIdByVisitId(int visitId) =>
            context.Examinations
                .Where(e => e.Visit_ID == visitId)
                .OrderByDescending(e => e.Exam_Time)
                .Select(e => (int?)e.Room_ID)
                .FirstOrDefault();

        public int? GetRoomIdByCurrentVisit(int visitId) =>
            context.ERRooms
                .Where(r => r.Current_Visit_ID == visitId)
                .Select(r => (int?)r.Room_ID)
                .FirstOrDefault();

        public int? GetAssignedRoomIdForVisit(int visitId) =>
            GetRoomIdByCurrentVisit(visitId) ?? GetRoomIdByVisitId(visitId);

        public ER_Visit? GetVisitByRoomId(int roomId)
        {
            int? currentVisitId = context.ERRooms
                .Where(r => r.Room_ID == roomId)
                .Select(r => r.Current_Visit_ID)
                .FirstOrDefault();

            if (currentVisitId.HasValue)
            {
                ER_Visit? visit = context.ERVisits
                    .AsNoTracking()
                    .FirstOrDefault(v => v.Visit_ID == currentVisitId.Value &&
                        v.Status != ER_Visit.VisitStatus.TRANSFERRED &&
                        v.Status != ER_Visit.VisitStatus.CLOSED);

                if (visit is not null)
                {
                    return visit;
                }
            }

            int? fallbackVisitId = context.Examinations
                .Where(e => e.Room_ID == roomId)
                .OrderByDescending(e => e.Exam_Time)
                .Select(e => (int?)e.Visit_ID)
                .FirstOrDefault();

            return fallbackVisitId.HasValue
                ? context.ERVisits.AsNoTracking().FirstOrDefault(v => v.Visit_ID == fallbackVisitId.Value)
                : null;
        }
    }
}
