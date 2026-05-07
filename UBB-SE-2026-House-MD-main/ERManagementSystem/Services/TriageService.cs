using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using HospitalManagement.Data;

namespace ERManagementSystem.Services
{
    public class TriageService : ITriageService
    {
        private readonly ITriageRepository triageRepository;
        private readonly ITriageParametersRepository triageParametersRepository;
        private readonly NurseService nurseService;
        private readonly IStateManagementService stateService;
        private readonly EFHospitalDbContext context;

        public TriageService(
            ITriageRepository triageRepository,
            ITriageParametersRepository triageParametersRepository,
            NurseService nurseService,
            IStateManagementService stateService,
            EFHospitalDbContext context)
        {
            this.triageRepository = triageRepository;
            this.triageParametersRepository = triageParametersRepository;
            this.nurseService = nurseService;
            this.stateService = stateService;
            this.context = context;
        }

        /// <summary>
        /// Requests an available nurse from the mock external Staff Management system.
        /// </summary>
        public int? RequestAvailableNurse()
        {
            return nurseService.RequestAvailableNurse();
        }

        /// <summary>
        /// Creates and persists the triage parameters record.
        /// </summary>
        public void CreateTriageParameters(Triage_Parameters parameters) // de schimbat la diagrama UML nu trebuie transmis si id-ul
        {
            triageParametersRepository.Add(parameters);
        }

        /// <summary>
        /// Creates the triage record using the triage parameters.
        /// This method:
        /// 1. Requests an available nurse
        /// 2. Calculates triage level
        /// 3. Determines specialization
        /// 4. Persists the triage parameters
        /// 5. Persists the triage record
        /// </summary>
        public Triage CreateTriage(int visitId, Triage_Parameters parameters)
            => CreateTriageAsync(visitId, parameters).GetAwaiter().GetResult();

        public async Task<Triage> CreateTriageAsync(int visitId, Triage_Parameters parameters)
        {
            parameters.ValidateParameters();

            Logger.Info($"[TriageService] Starting triage for visit {visitId}");

            try
            {
                Triage? existingTriage = await triageRepository.GetByVisitIdAsync(visitId);
                if (existingTriage != null)
                {
                    Triage_Parameters? existingParameters =
                        await triageParametersRepository.GetByTriageIdAsync(existingTriage.Triage_ID);

                    if (existingParameters != null)
                    {
                        Logger.Warning($"[TriageService] Visit {visitId} already has triage {existingTriage.Triage_ID}.");
                        throw new InvalidOperationException("Triage has already been performed for this visit.");
                    }

                    Logger.Warning($"[TriageService] Removing orphaned triage {existingTriage.Triage_ID} for visit {visitId} before retry.");
                    await triageRepository.DeleteAsync(existingTriage);
                }

                int? nurseId = RequestAvailableNurse();
                if (nurseId == null)
                {
                    Logger.Warning($"[TriageService] No available nurse for visit {visitId}");
                    throw new InvalidOperationException("No available nurse.");
                }

                int triageLevel = CalculateTriageLevel(parameters);
                string specialization = DetermineSpecialization(parameters);

                Logger.Info($"[TriageService] Calculated level {triageLevel}, specialization {specialization} for visit {visitId}");

                await using var transaction = await context.Database.BeginTransactionAsync();

                Triage triage = new Triage
                {
                    Visit_ID = visitId,
                    Triage_Level = triageLevel,
                    Specialization = specialization,
                    Nurse_ID = nurseId.Value,
                    Triage_Time = DateTime.Now
                };

                int triageId = await triageRepository.AddAsync(triage);

                parameters.Triage_ID = triageId;
                await triageParametersRepository.AddAsync(parameters);

                triage.Triage_ID = triageId;

                await stateService.ChangeVisitStatusAsync(visitId, ER_Visit.VisitStatus.TRIAGED);
                await transaction.CommitAsync();

                Logger.Info($"[TriageService] Completed triage {triageId} for visit {visitId}");

                return triage;
            }
            catch (Exception ex)
            {
                Logger.Error($"[TriageService] Failed triage process for visit {visitId}", ex);
                throw;
            }
        }

        public Triage? GetByVisitId(int visitId)
            => GetByVisitIdAsync(visitId).GetAwaiter().GetResult();

        public Task<Triage?> GetByVisitIdAsync(int visitId)
            => triageRepository.GetByVisitIdAsync(visitId);

        public IReadOnlyList<ER_Visit> GetVisitsForTriage()
            => GetVisitsForTriageAsync().GetAwaiter().GetResult();

        public async Task<IReadOnlyList<ER_Visit>> GetVisitsForTriageAsync()
        {
            return (await stateService.GetByStatusAsync(ER_Visit.VisitStatus.REGISTERED))
                .Concat(await stateService.GetByStatusAsync(ER_Visit.VisitStatus.TRIAGED))
                .OrderBy(visit => visit.Arrival_date_time)
                .ToList();
        }

        public void MoveVisitToQueue(int visitId)
            => MoveVisitToQueueAsync(visitId).GetAwaiter().GetResult();

        public Task MoveVisitToQueueAsync(int visitId)
            => stateService.ChangeVisitStatusAsync(visitId, ER_Visit.VisitStatus.WAITING_FOR_ROOM);

        public void CloseVisit(int visitId)
            => CloseVisitAsync(visitId).GetAwaiter().GetResult();

        public Task CloseVisitAsync(int visitId)
            => stateService.CloseVisitAsync(visitId);

        /// <summary>
        /// Calculates the triage level entirely in C#.
        ///
        /// Step 1: Critical condition check
        /// - If consciousness = 3 OR breathing = 3 OR injury_type = 3 OR bleeding = 3
        ///   => triage level = 1 (highest priority)
        ///
        /// Step 2: Weighted severity score
        /// severity_score =
        ///   (consciousness x 3) +
        ///   (breathing x 3) +
        ///   (bleeding x 2) +
        ///   (injury_type x 2) +
        ///   (pain_level x 1)
        ///
        ///
        ///  <=11   => Level 5
        /// - 12-15 => Level 4
        /// - 16-19 => Level 3
        /// - >=20      => Level 2
        ///
        /// Adjust these thresholds if your project documentation uses different cutoffs.
        /// </summary>
        public int CalculateTriageLevel(Triage_Parameters parameters)
        {
            if (parameters.Consciousness == 3 ||
                parameters.Breathing == 3 ||
                parameters.Injury_Type == 3 ||
                parameters.Bleeding == 3)
            {
                return 1;
            }

            int severityScore =
                (parameters.Consciousness * 3) +
                (parameters.Breathing * 3) +
                (parameters.Bleeding * 2) +
                (parameters.Injury_Type * 2) +
                (parameters.Pain_Level * 1);

            if (severityScore >= 20)
            {
                return 2;
            }

            if (severityScore >= 16)
            {
                return 3;
            }

            if (severityScore >= 12)
            {
                return 4;
            }

            return 5;
        }

        /// <summary>
        /// Determines the specialization for the triage record.
        ///
        /// Rules:
        /// - injury_type = 2 -> Orthopedics
        /// - breathing = 2 -> Pulmonology
        /// - consciousness = 2 OR 3 -> Neurology
        /// - bleeding = 3 OR injury_type = 3 -> General Surgery
        /// - else -> Emergency Medicine
        ///
        /// Note: This follows the rule order exactly as given.
        /// </summary>
        public string DetermineSpecialization(Triage_Parameters parameters)
        {
            if (parameters.Bleeding == 3 || parameters.Injury_Type == 3)
            {
                return "General Surgery";
            }

            if (parameters.Injury_Type == 2)
            {
                return "Orthopedics";
            }

            if (parameters.Breathing == 2)
            {
                return "Pulmonology";
            }

            if (parameters.Consciousness == 2 || parameters.Consciousness == 3)
            {
                return "Neurology";
            }

            return "Emergency Medicine";
        }
    }
}
