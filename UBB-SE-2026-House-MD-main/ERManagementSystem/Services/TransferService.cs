using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    /// <summary>
    /// Tasks 6.4, 6.5, 6.6, 6.10, 6.11, 6.12
    ///   SendPatientData(visitId: int): Transfer_Log
    ///   LogTransfer(int, status: string): void
    ///   GetLogs(visitId: int): List
    ///
    /// Feature 8: simulates external API call by saving JSON to a local file
    /// Task 6.5: stores the file path in the Transfer_Log table
    /// Feature 9: every attempt is logged with SUCCESS / FAILED / RETRYING
    /// </summary>
    public class TransferService : ITransferService
    {
        private readonly SqlHelper sqlHelper;
        private readonly ITransferLogRepository transferLogRepository;
        private readonly string transferDirectory;

        public const string TARGET_SYSTEM = "Patient Management";

        private readonly IStateManagementService stateManagementService;

        public TransferService(
            SqlHelper sqlHelper,
            ITransferLogRepository transferLogRepository,
            IStateManagementService stateManagementService)
        {
            this.sqlHelper = sqlHelper;
            this.transferLogRepository = transferLogRepository;
            transferDirectory = Path.Combine(AppContext.BaseDirectory, "transfers");
            this.stateManagementService = stateManagementService;
            Directory.CreateDirectory(transferDirectory);
        }

        // SendPatientData(visitId: int): Transfer_Log
        // Tasks 6.4 & 6.5

        /// <summary>
        /// Builds the patient data package, serializes to JSON, saves to local file
        /// Task 6.5: stores the file path in the log entry
        /// Logs the attempt. Returns the log entry
        /// </summary>
        public Transfer_Log SendPatientData(int visitId)
        {
            var log = new Transfer_Log
            {
                Visit_ID = visitId,
                Transfer_Time = DateTime.Now,
                Target_System = TARGET_SYSTEM,
                Status = "FAILED"
            };

            try
            {
                // Task 6.3 — build full data package via JOIN query
                var package = BuildPatientDataPackage(visitId);

                // Task 6.5 — serialize to JSON and save to local file
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(package, options);
                string fileName = $"transfer_visit_{visitId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(transferDirectory, fileName);
                File.WriteAllText(filePath, json);

                // Task 6.5: store the file path in the log
                log.FilePath = filePath;
                Logger.Info($"[TransferService] Patient data for Visit {visitId} saved to {filePath}");
                log.Status = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Status = "FAILED";
                log.FilePath = null;
                Logger.Error($"[TransferService] SendPatientData failed for Visit {visitId}", ex);
                transferLogRepository.Add(log);
                throw;
            }

            // Task 6.6 — persist log entry
            transferLogRepository.Add(log);
            return log;
        }

        // LogTransfer(int, status: string): void
        // Task 6.6

        /// <summary>
        /// Creates and persists a Transfer_Log entry with the given visitId and status.
        /// Each attempt is a separate log row.
        /// </summary>
        public void LogTransfer(int visitId, string status)
        {
            var log = new Transfer_Log
            {
                Visit_ID = visitId,
                Transfer_Time = DateTime.Now,
                Target_System = TARGET_SYSTEM,
                Status = status
            };
            log.Validate();
            Logger.Info($"[TransferService] Transfer logged for Visit {visitId} with status '{status}'");
            transferLogRepository.Add(log);
        }

        // GetLogs(visitId: int): List
        // Task 6.12

        /// <summary>
        /// Returns all Transfer_Log entries for a visit.
        /// Called by TransferLogViewModel.LoadLogs().
        /// </summary>
        public List<Transfer_Log> GetLogs(int visitId)
        {
            return transferLogRepository.GetByVisitId(visitId);
        }

        // Task 6.11 — Retry mechanism

        /// <summary>
        /// Logs a RETRYING entry, then re-attempts SendPatientData.
        /// Each attempt is a separate log row.
        /// </summary>
        public Transfer_Log RetryTransfer(int visitId)
        {
            Logger.Warning($"[TransferService] Retrying transfer for Visit {visitId}");
            LogTransfer(visitId, "RETRYING");
            return SendPatientData(visitId);
        }

        // Task 6.10 — Mark Patient.Transferred = true

        /// <summary>
        /// Sets Patient.Transferred = 1 after a successful transfer.
        /// Hand-written UPDATE query via SqlHelper.
        /// </summary>
        public void MarkPatientAsTransferred(int visitId)
        {
            const string sql = @"
                UPDATE dbo.Patient
                SET Transferred = 1
                WHERE Patient_ID = (
                    SELECT Patient_ID FROM dbo.ER_Visit WHERE Visit_ID = @VisitId
                )";

            sqlHelper.ExecuteNonQuery(sql,
                new SqlParameter("@VisitId", visitId));
            Logger.Info($"[TransferService] Patient marked as transferred for Visit {visitId}");
        }

        // Task 6.10 — Transition visit IN_EXAMINATION → TRANSFERRED
        public void TransitionVisitToTransferred(int visitId)
        {
            stateManagementService.ChangeVisitStatus(visitId, ER_Visit.VisitStatus.TRANSFERRED);
            Logger.Info($"[TransferService] Visit {visitId} transitioned to TRANSFERRED");
        }

        public void CloseVisit(int visitId)
        {
            stateManagementService.CloseVisit(visitId);
            Logger.Info($"[TransferService] Visit {visitId} transitioned to CLOSED");
        }

        public List<TransferEligibleVisit> GetEligibleVisitsForTransfer()
        {
            const string sql = @"
                SELECT v.Visit_ID,
                       v.Chief_Complaint,
                       v.Status,
                       p.First_Name,
                       p.Last_Name,
                       p.Transferred
                FROM dbo.ER_Visit v
                INNER JOIN dbo.Patient p ON p.Patient_ID = v.Patient_ID
                WHERE v.Status = @Status
                ORDER BY v.Arrival_date_time ASC";

            var eligibleVisits = new List<TransferEligibleVisit>();
            using var reader = sqlHelper.ExecuteReader(
                sql,
                new SqlParameter("@Status", ER_Visit.VisitStatus.IN_EXAMINATION));

            while (reader.Read())
            {
                eligibleVisits.Add(new TransferEligibleVisit
                {
                    VisitId = reader.GetInt32(0),
                    ChiefComplaint = reader.GetString(1),
                    Status = reader.GetString(2),
                    PatientFirstName = reader.GetString(3),
                    PatientLastName = reader.GetString(4),
                    IsTransferred = reader.GetBoolean(5)
                });
            }

            return eligibleVisits;
        }

        // Task 6.3 — Build PatientDataPackage via hand-written JOIN query
        private PatientDataPackage BuildPatientDataPackage(int visitId)
        {
            const string sql = @"
                SELECT
                    p.Patient_ID,
                    p.First_Name,
                    p.Last_Name,
                    p.Date_of_Birth,
                    p.Gender,
                    p.Phone,
                    p.Emergency_Contact,
                    v.Visit_ID,
                    v.Arrival_date_time,
                    v.Chief_Complaint,
                    t.Triage_Level,
                    t.Specialization,
                    t.Nurse_ID,
                    tp.Consciousness,
                    tp.Breathing,
                    tp.Bleeding,
                    tp.Injury_Type,
                    tp.Pain_Level,
                    e.Exam_Time,
                    e.Notes,
                    e.Doctor_ID
                FROM dbo.ER_Visit v
                INNER JOIN dbo.Patient           p  ON p.Patient_ID  = v.Patient_ID
                LEFT  JOIN dbo.Triage            t  ON t.Visit_ID    = v.Visit_ID
                LEFT  JOIN dbo.Triage_Parameters tp ON tp.Triage_ID  = t.Triage_ID
                LEFT  JOIN dbo.Examination       e  ON e.Visit_ID    = v.Visit_ID
                WHERE v.Visit_ID = @VisitId";

            using var reader = sqlHelper.ExecuteReader(sql,
                new SqlParameter("@VisitId", visitId));

            if (!reader.Read())
            {
                throw new InvalidOperationException($"No visit found with ID {visitId}.");
            }

            return new PatientDataPackage
            {
                CNP = reader.GetString(reader.GetOrdinal("Patient_ID")),
                First_Name = reader.GetString(reader.GetOrdinal("First_Name")),
                Last_Name = reader.GetString(reader.GetOrdinal("Last_Name")),
                Date_of_Birth = reader.GetDateTime(reader.GetOrdinal("Date_of_Birth")),
                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                Emergency_Contact = reader.GetString(reader.GetOrdinal("Emergency_Contact")),
                Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                Arrival_date_time = reader.GetDateTime(reader.GetOrdinal("Arrival_date_time")),
                Chief_Complaint = reader.GetString(reader.GetOrdinal("Chief_Complaint")),
                Triage_Level = reader.IsDBNull(reader.GetOrdinal("Triage_Level")) ? 0 : reader.GetInt32(reader.GetOrdinal("Triage_Level")),
                Specialization = reader.IsDBNull(reader.GetOrdinal("Specialization")) ? string.Empty : reader.GetString(reader.GetOrdinal("Specialization")),
                Nurse_ID = reader.IsDBNull(reader.GetOrdinal("Nurse_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("Nurse_ID")),
                Consciousness = reader.IsDBNull(reader.GetOrdinal("Consciousness")) ? 0 : reader.GetInt32(reader.GetOrdinal("Consciousness")),
                Breathing = reader.IsDBNull(reader.GetOrdinal("Breathing")) ? 0 : reader.GetInt32(reader.GetOrdinal("Breathing")),
                Bleeding = reader.IsDBNull(reader.GetOrdinal("Bleeding")) ? 0 : reader.GetInt32(reader.GetOrdinal("Bleeding")),
                Injury_Type = reader.IsDBNull(reader.GetOrdinal("Injury_Type")) ? 0 : reader.GetInt32(reader.GetOrdinal("Injury_Type")),
                Pain_Level = reader.IsDBNull(reader.GetOrdinal("Pain_Level")) ? 0 : reader.GetInt32(reader.GetOrdinal("Pain_Level")),
                Exam_Time = reader.IsDBNull(reader.GetOrdinal("Exam_Time")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Exam_Time")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                Doctor_ID = reader.IsDBNull(reader.GetOrdinal("Doctor_ID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Doctor_ID"))
            };
        }
    }
}