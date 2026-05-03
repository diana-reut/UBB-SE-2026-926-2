using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    /// <summary>
    /// Task 6.2 - TransferLogRepository.
    ///   Add(log: Transfer_Log): void
    ///   GetByVisitId(id: int): List
    ///   GetAll(): List
    ///   DeleteLog(log: Transfer_Log): void
    ///
    /// Uses SqlHelper. Hand-written SQL only. No ORM.
    /// All SqlDataReader mapping is manual.
    /// </summary>
    public class TransferLogRepository : ITransferLogRepository
    {
        private readonly SqlHelper sqlHelper;

        public TransferLogRepository(SqlHelper sqlHelper)
        {
            this.sqlHelper = sqlHelper;
        }

        /// <summary>
        /// Executes a parameterized INSERT INTO Transfer_Log.
        /// Sets log.Transfer_ID to the generated identity value.
        /// </summary>
        public void Add(Transfer_Log log)
        {
            const string sql = @"
            INSERT INTO dbo.Transfer_Log (Visit_ID, Transfer_Time, Target_System, Status, FilePath)
            OUTPUT INSERTED.Transfer_ID
            VALUES (@VisitId, @TransferTime, @TargetSystem, @Status, @FilePath)";

            var parameters = new[]
            {
                new SqlParameter("@VisitId",      log.Visit_ID),
                new SqlParameter("@TransferTime", log.Transfer_Time),
                new SqlParameter("@TargetSystem", log.Target_System),
                new SqlParameter("@Status",       log.Status),
                new SqlParameter("@FilePath", (object?)log.FilePath ?? DBNull.Value)
            };

            using var reader = sqlHelper.ExecuteReader(sql, parameters);
            if (reader.Read())
            {
                log.Transfer_ID = reader.GetInt32(0);
            }

            Logger.Info($"[TransferLogRepository] Added log entry {log.Transfer_ID} for Visit {log.Visit_ID}, Status={log.Status}");
        }

        // GetByVisitId(id: int): List

        /// <summary>
        /// SELECT all Transfer_Log entries for a given Visit_ID.
        /// Results ordered newest first.
        /// </summary>
        public List<Transfer_Log> GetByVisitId(int id)
        {
            const string sql = @"
                SELECT Transfer_ID, Visit_ID, Transfer_Time, Target_System, Status
                FROM dbo.Transfer_Log
                WHERE Visit_ID = @VisitId
                ORDER BY Transfer_Time DESC";

            var logs = new List<Transfer_Log>();
            using var reader = sqlHelper.ExecuteReader(sql,
                new SqlParameter("@VisitId", id));

            while (reader.Read())
            {
                logs.Add(MapFromReader(reader));
            }

            return logs;
        }

        // GetAll(): List

        /// <summary>
        /// SELECT all Transfer_Log entries across all visits.
        /// Ordered newest first.
        /// </summary>
        public List<Transfer_Log> GetAll()
        {
            const string sql = @"
                SELECT Transfer_ID, Visit_ID, Transfer_Time, Target_System, Status
                FROM dbo.Transfer_Log
                ORDER BY Transfer_Time DESC";

            var logs = new List<Transfer_Log>();
            using var reader = sqlHelper.ExecuteReader(sql);
            while (reader.Read())
            {
                logs.Add(MapFromReader(reader));
            }

            return logs;
        }

        // DeleteLog(log: Transfer_Log): void

        /// <summary>
        /// DELETE a Transfer_Log entry by its Transfer_ID.
        /// </summary>
        public void DeleteLog(Transfer_Log log)
        {
            const string sql = "DELETE FROM dbo.Transfer_Log WHERE Transfer_ID = @TransferId";

            sqlHelper.ExecuteNonQuery(sql,
                new SqlParameter("@TransferId", log.Transfer_ID));
            Logger.Info($"[TransferLogRepository] Deleted log entry {log.Transfer_ID}");
        }

        // UpdateStatus — internal helper used by retry mechanism
        public void UpdateStatus(int transferId, string newStatus)
        {
            const string sql = @"
                UPDATE dbo.Transfer_Log
                SET Status = @Status
                WHERE Transfer_ID = @TransferId";

            sqlHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Status", newStatus),
                new SqlParameter("@TransferId", transferId));
            Logger.Info($"[TransferLogRepository] Updated log {transferId} to status '{newStatus}'");
        }

        // Manual SqlDataReader → Transfer_Log mapping
        private static Transfer_Log MapFromReader(SqlDataReader reader)
        {
            return new Transfer_Log
            {
                Transfer_ID = reader.GetInt32(reader.GetOrdinal("Transfer_ID")),
                Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                Transfer_Time = reader.GetDateTime(reader.GetOrdinal("Transfer_Time")),
                Target_System = reader.GetString(reader.GetOrdinal("Target_System")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            };
        }
    }
}