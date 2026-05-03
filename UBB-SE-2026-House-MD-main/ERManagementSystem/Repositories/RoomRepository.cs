using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly SqlHelper sqlHelper;

        public RoomRepository(SqlHelper sqlHelper)
        {
            this.sqlHelper = sqlHelper;
        }

        public List<ER_Room> GetAllRooms()
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status, Current_Visit_ID
                FROM   dbo.ER_Room";

            var rooms = new List<ER_Room>();
            try
            {
                using var reader = sqlHelper.ExecuteReader(query);
                while (reader.Read())
                {
                    rooms.Add(MapReaderToRoom(reader));
                }

                Logger.Info($"GetAllRooms returned {rooms.Count} room(s).");
            }
            catch (Exception ex)
            {
                Logger.Error("DB error in RoomRepository.GetAllRooms.", ex);
                throw;
            }
            return rooms;
        }

        public ER_Room? GetById(int roomId)
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status, Current_Visit_ID
                FROM   dbo.ER_Room
                WHERE  Room_ID = @Room_ID";

            var parameters = new[] { new SqlParameter("@Room_ID", roomId) };

            try
            {
                using var reader = sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    return MapReaderToRoom(reader);
                }

                Logger.Warning($"RoomRepository.GetById: Room {roomId} not found.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetById for Room {roomId}.", ex);
                throw;
            }
        }

        public List<ER_Room> GetAvailableRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Available);
        public List<ER_Room> GetOccupiedRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Occupied);
        public List<ER_Room> GetCleaningRooms() => GetRoomsByStatus(ER_Room.RoomStatus.Cleaning);

        public List<ER_Room> GetRoomsByStatus(string status)
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status, Current_Visit_ID
                FROM   dbo.ER_Room
                WHERE  Availability_Status = @Status";

            var parameters = new[] { new SqlParameter("@Status", status) };
            var rooms = new List<ER_Room>();

            try
            {
                using var reader = sqlHelper.ExecuteReader(query, parameters);
                while (reader.Read())
                {
                    rooms.Add(MapReaderToRoom(reader));
                }

                Logger.Info($"GetRoomsByStatus('{status}') returned {rooms.Count} room(s).");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetRoomsByStatus('{status}').", ex);
                throw;
            }
            return rooms;
        }

        public void UpdateAvailabilityStatus(int roomId, string newStatus)
        {
            const string query = @"
                UPDATE dbo.ER_Room
                SET    Availability_Status = @Status
                WHERE  Room_ID = @Room_ID";

            var parameters = new[]
            {
                new SqlParameter("@Status",  newStatus),
                new SqlParameter("@Room_ID", roomId)
            };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Room {roomId} status updated to '{newStatus}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.UpdateAvailabilityStatus for Room {roomId}.", ex);
                throw;
            }
        }

        /// <summary>Records which visit is currently in this room (called on assignment).</summary>
        public void SetCurrentVisit(int roomId, int visitId)
        {
            const string query = @"
                UPDATE dbo.ER_Room
                SET    Current_Visit_ID = @Visit_ID
                WHERE  Room_ID = @Room_ID";

            var parameters = new[]
            {
                new SqlParameter("@Visit_ID", visitId),
                new SqlParameter("@Room_ID",  roomId)
            };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Room {roomId} linked to Visit {visitId}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.SetCurrentVisit for Room {roomId}.", ex);
                throw;
            }
        }

        /// <summary>Clears the visit link when the room moves to 'cleaning'.</summary>
        public void ClearCurrentVisit(int roomId)
        {
            const string query = @"
                UPDATE dbo.ER_Room
                SET    Current_Visit_ID = NULL
                WHERE  Room_ID = @Room_ID";

            var parameters = new[] { new SqlParameter("@Room_ID", roomId) };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Room {roomId} visit link cleared.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.ClearCurrentVisit for Room {roomId}.", ex);
                throw;
            }
        }

        /// <summary>
        /// Task 5.13 — Returns the Room_ID linked to a visit via the Examination table.
        /// Lives here instead of ExaminationRepository because it is a room-domain query.
        /// Returns null if no examination record exists yet for the visit.
        /// </summary>
        public int? GetRoomIdByVisitId(int visitId)
        {
            const string query = @"
                SELECT TOP 1 Room_ID
                FROM   dbo.Examination
                WHERE  Visit_ID = @Visit_ID
                ORDER BY Exam_Time DESC";

            var parameters = new[] { new SqlParameter("@Visit_ID", visitId) };

            try
            {
                using var reader = sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    int roomId = reader.GetInt32(0);
                    Logger.Info($"GetRoomIdByVisitId: Visit {visitId} → Room {roomId}.");
                    return roomId;
                }

                Logger.Warning($"GetRoomIdByVisitId: No examination found for Visit {visitId}.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetRoomIdByVisitId for Visit {visitId}.", ex);
                throw;
            }
        }

        /// <summary>
        /// Finds the Room_ID whose Current_Visit_ID matches the given visit.
        /// Used by StateManagementService as a fallback when no Examination record exists yet
        /// (e.g. patient is IN_ROOM or WAITING_FOR_DOCTOR when transferred/closed).
        /// </summary>
        public int? GetRoomIdByCurrentVisit(int visitId)
        {
            const string query = @"
                SELECT Room_ID
                FROM   dbo.ER_Room
                WHERE  Current_Visit_ID = @Visit_ID";

            var parameters = new[] { new SqlParameter("@Visit_ID", visitId) };

            try
            {
                using var reader = sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    int roomId = reader.GetInt32(0);
                    Logger.Info($"GetRoomIdByCurrentVisit: Visit {visitId} → Room {roomId}.");
                    return roomId;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetRoomIdByCurrentVisit for Visit {visitId}.", ex);
                return null;   // non-fatal — auto-clean fallback
            }
        }

        /// <summary>
        /// Task 5.13 — Finds the occupied Room_ID that was assigned via RoomAssignmentService
        /// for the given visit. Checks the Examination table first (if an earlier record exists),
        /// then falls back to any occupied room whose ID matches a prior assignment for this visit
        /// by looking up the most recent Examination record tied to the same patient/visit chain.
        /// Used by ExaminationViewModel to store the correct Room_ID in new Examination records.
        /// <summary>
        /// Used by ExaminationViewModel.SaveExamination() to find the correct Room_ID.
        /// Primary:   Current_Visit_ID on ER_Room (set at assignment — always accurate).
        /// Secondary: Examination table (re-save scenario where exam already exists).
        /// Returns null if neither source has a record (caller falls back to GetFirstRoomId).
        /// </summary>
        public int? GetAssignedRoomIdForVisit(int visitId)
        {
            // Primary: Current_Visit_ID — set the moment a room is assigned to this visit
            int? fromCurrentVisit = GetRoomIdByCurrentVisit(visitId);
            if (fromCurrentVisit.HasValue)
            {
                Logger.Info($"GetAssignedRoomIdForVisit: Visit {visitId} → Room {fromCurrentVisit.Value} (Current_Visit_ID).");
                return fromCurrentVisit;
            }

            // Secondary: Examination table (re-save or visit that was assigned pre-migration)
            int? fromExam = GetRoomIdByVisitId(visitId);
            if (fromExam.HasValue)
            {
                Logger.Info($"GetAssignedRoomIdForVisit: Visit {visitId} → Room {fromExam.Value} (Examination fallback).");
                return fromExam;
            }

            Logger.Warning($"GetAssignedRoomIdForVisit: no room found for Visit {visitId}.");
            return null;
        }

        /// <summary>
        /// VisitDetailsPanel — finds the active visit in a given room.
        /// Primary: queries Current_Visit_ID (set at assignment time, always accurate).
        /// Fallback: Examination table (for legacy seeded rows with no Current_Visit_ID).
        /// </summary>
        public ER_Visit? GetVisitByRoomId(int roomId)
        {
            // Primary: use Current_Visit_ID — works even before any Examination record exists
            const string primaryQuery = @"
                SELECT v.Visit_ID, v.Patient_ID, v.Arrival_date_time,
                       v.Chief_Complaint, v.Status
                FROM   dbo.ER_Room r
                INNER JOIN dbo.ER_Visit v ON v.Visit_ID = r.Current_Visit_ID
                WHERE  r.Room_ID = @Room_ID
                  AND  v.Status NOT IN ('TRANSFERRED','CLOSED')";

            var parameters = new[] { new SqlParameter("@Room_ID", roomId) };

            try
            {
                using var reader = sqlHelper.ExecuteReader(primaryQuery, parameters);
                if (reader.Read())
                {
                    Logger.Info($"GetVisitByRoomId: Room {roomId} → Visit {reader.GetInt32(0)} (Current_Visit_ID).");
                    return new ER_Visit
                    {
                        Visit_ID = reader.GetInt32(0),
                        Patient_ID = reader.GetString(1),
                        Arrival_date_time = reader.GetDateTime(2),
                        Chief_Complaint = reader.GetString(3),
                        Status = reader.GetString(4)
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetVisitByRoomId (primary) for Room {roomId}.", ex);
            }

            // Fallback: Examination table for rooms with no Current_Visit_ID set yet
            const string fallbackQuery = @"
                SELECT TOP 1 v.Visit_ID, v.Patient_ID, v.Arrival_date_time,
                             v.Chief_Complaint, v.Status
                FROM   dbo.Examination e
                INNER JOIN dbo.ER_Visit v ON v.Visit_ID = e.Visit_ID
                WHERE  e.Room_ID = @Room_ID
                  AND  v.Status NOT IN ('TRANSFERRED','CLOSED')
                ORDER BY e.Exam_Time DESC";

            try
            {
                using var reader = sqlHelper.ExecuteReader(fallbackQuery, parameters);
                if (reader.Read())
                {
                    Logger.Info($"GetVisitByRoomId: Room {roomId} → Visit {reader.GetInt32(0)} (Examination fallback).");
                    return new ER_Visit
                    {
                        Visit_ID = reader.GetInt32(0),
                        Patient_ID = reader.GetString(1),
                        Arrival_date_time = reader.GetDateTime(2),
                        Chief_Complaint = reader.GetString(3),
                        Status = reader.GetString(4)
                    };
                }
                Logger.Warning($"GetVisitByRoomId: no active visit found for Room {roomId}.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in RoomRepository.GetVisitByRoomId (fallback) for Room {roomId}.", ex);
                return null;
            }
        }

        private static ER_Room MapReaderToRoom(SqlDataReader reader) => new ER_Room
        {
            Room_ID = Convert.ToInt32(reader["Room_ID"]),
            Room_Type = reader["Room_Type"].ToString() !,
            Availability_Status = reader["Availability_Status"].ToString() !,
            Current_Visit_ID = reader["Current_Visit_ID"] is DBNull
                                  ? null
                                  : Convert.ToInt32(reader["Current_Visit_ID"])
        };
    }
}
