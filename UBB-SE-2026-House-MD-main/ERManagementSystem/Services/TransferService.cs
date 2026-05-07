using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Entity;

namespace ERManagementSystem.Services
{
    public class TransferService : ITransferService
    {
        private readonly EFHospitalDbContext context;
        private readonly ITransferLogRepository transferLogRepository;
        private readonly string transferDirectory;
        private readonly IStateManagementService stateManagementService;

        public const string TARGET_SYSTEM = "Patient Management";

        public TransferService(
            EFHospitalDbContext context,
            ITransferLogRepository transferLogRepository,
            IStateManagementService stateManagementService)
        {
            this.context = context;
            this.transferLogRepository = transferLogRepository;
            this.stateManagementService = stateManagementService;
            transferDirectory = Path.Combine(AppContext.BaseDirectory, "transfers");
            Directory.CreateDirectory(transferDirectory);
        }

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
                PatientDataPackage package = BuildPatientDataPackage(visitId);
                string json = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
                string fileName = $"transfer_visit_{visitId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(transferDirectory, fileName);
                File.WriteAllText(filePath, json);
                log.FilePath = filePath;
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

            transferLogRepository.Add(log);
            return log;
        }

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
            transferLogRepository.Add(log);
        }

        public List<Transfer_Log> GetLogs(int visitId) => transferLogRepository.GetByVisitId(visitId);

        public Transfer_Log RetryTransfer(int visitId)
        {
            LogTransfer(visitId, "RETRYING");
            return SendPatientData(visitId);
        }

        public void MarkPatientAsTransferred(int visitId)
        {
            string? cnp = context.ERVisits
                .Where(v => v.Visit_ID == visitId)
                .Select(v => v.Patient_ID)
                .FirstOrDefault();

            if (cnp is null)
            {
                return;
            }

            Patient patient = context.Patients.First(p => p.Cnp == cnp);
            patient.Transferred = true;
            context.SaveChanges();
        }

        public void TransitionVisitToTransferred(int visitId)
        {
            stateManagementService.ChangeVisitStatus(visitId, ER_Visit.VisitStatus.TRANSFERRED);
        }

        public void CloseVisit(int visitId)
        {
            stateManagementService.CloseVisit(visitId);
        }

        public List<TransferEligibleVisit> GetEligibleVisitsForTransfer()
        {
            return (
                from visit in context.ERVisits
                join patient in context.Patients on visit.Patient_ID equals patient.Cnp
                where visit.Status == ER_Visit.VisitStatus.IN_EXAMINATION
                orderby visit.Arrival_date_time
                select new TransferEligibleVisit
                {
                    VisitId = visit.Visit_ID,
                    ChiefComplaint = visit.Chief_Complaint,
                    Status = visit.Status,
                    PatientFirstName = patient.FirstName,
                    PatientLastName = patient.LastName,
                    IsTransferred = patient.Transferred,
                }).ToList();
        }

        private PatientDataPackage BuildPatientDataPackage(int visitId)
        {
            var package = (
                from visit in context.ERVisits
                join patient in context.Patients on visit.Patient_ID equals patient.Cnp
                join triage in context.Triages on visit.Visit_ID equals triage.Visit_ID into triageJoin
                from triage in triageJoin.DefaultIfEmpty()
                join parameters in context.TriageParameters on triage.Triage_ID equals parameters.Triage_ID into paramsJoin
                from parameters in paramsJoin.DefaultIfEmpty()
                join exam in context.Examinations on visit.Visit_ID equals exam.Visit_ID into examJoin
                from exam in examJoin
                    .OrderByDescending(e => e.Exam_Time)
                    .Take(1)
                    .DefaultIfEmpty()
                where visit.Visit_ID == visitId
                select new PatientDataPackage
                {
                    CNP = patient.Cnp,
                    First_Name = patient.FirstName,
                    Last_Name = patient.LastName,
                    Date_of_Birth = patient.Dob,
                    Gender = patient.Sex == Common.Data.Entity.Enums.Sex.F ? "Female" : "Male",
                    Phone = patient.PhoneNo,
                    Emergency_Contact = patient.EmergencyContact,
                    Visit_ID = visit.Visit_ID,
                    Arrival_date_time = visit.Arrival_date_time,
                    Chief_Complaint = visit.Chief_Complaint,
                    Triage_Level = triage != null ? triage.Triage_Level : 0,
                    Specialization = triage != null ? triage.Specialization : string.Empty,
                    Nurse_ID = triage != null ? triage.Nurse_ID : 0,
                    Consciousness = parameters != null ? parameters.Consciousness : 0,
                    Breathing = parameters != null ? parameters.Breathing : 0,
                    Bleeding = parameters != null ? parameters.Bleeding : 0,
                    Injury_Type = parameters != null ? parameters.Injury_Type : 0,
                    Pain_Level = parameters != null ? parameters.Pain_Level : 0,
                    Exam_Time = exam != null ? exam.Exam_Time : null,
                    Notes = exam != null ? exam.Notes : null,
                    Doctor_ID = exam != null ? exam.Doctor_ID : null,
                }).FirstOrDefault();

            return package ?? throw new InvalidOperationException($"No visit found with ID {visitId}.");
        }
    }
}
