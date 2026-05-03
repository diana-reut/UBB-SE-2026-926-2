using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Helpers;

namespace ERManagementSystem.Services
{
    public class ExaminationService : IExaminationService
    {
        private readonly IExaminationRepository examRepository;
        private readonly IERVisitRepository erVisitRepository;
        private readonly ITriageRepository triageRepository;
        private readonly MockStaffService mockStaffService;
        private readonly IStateManagementService stateManagementService;
        private readonly ITriageParametersRepository triageParamsRepo;

        public ExaminationService(
            IExaminationRepository examRepository,
            IERVisitRepository erVisitRepository,
            ITriageRepository triageRepository,
            MockStaffService mockStaffService,
            IStateManagementService stateManagementService,
            ITriageParametersRepository triageParamsRepo)
        {
            this.examRepository = examRepository;
            this.erVisitRepository = erVisitRepository;
            this.triageRepository = triageRepository;
            this.mockStaffService = mockStaffService;
            this.stateManagementService = stateManagementService;
            this.triageParamsRepo = triageParamsRepo;
        }

        /// <summary>
        /// Task 4.5: Doctor Request Workflow
        /// (1) Read triage record for the visit
        /// (2) Send specialization + symptoms to MockStaffService
        /// (3) Receive assigned Doctor_ID
        /// (4) Transition visit from IN_ROOM → WAITING_FOR_DOCTOR
        /// </summary>
        public int RequestDoctor(int visitID)
        {
            var triage = triageRepository.GetByVisitId(visitID);

            if (triage == null)
            {
                Logger.Warning($"RequestDoctor failed: Triage record missing for Visit {visitID}");
                throw new Exception($"Triage record not found for visit {visitID}");
            }

            var triageParameters = triageParamsRepo.GetByTriageId(triage.Triage_ID);

            if (triageParameters == null)
            {
                Logger.Warning($"RequestDoctor failed: Triage parameters missing for Triage {triage.Triage_ID}");
                throw new Exception($"Triage parameters not found for triage {triage.Triage_ID}");
            }

            int assignedDoctorId = mockStaffService.RequestDoctor(
                triage.Specialization,
                triageParameters);

            stateManagementService.ChangeVisitStatus(visitID, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR);
            Logger.Info($"Visit {visitID} transitioned to WAITING_FOR_DOCTOR.");

            return assignedDoctorId;
        }

        /// <summary>
        /// Task 4.4 / 4.8: Persists the examination record and transitions
        /// the visit from WAITING_FOR_DOCTOR → IN_EXAMINATION.
        /// </summary>
        public void SaveExamination(Examination exam)
        {
            examRepository.Add(exam);
            stateManagementService.ChangeVisitStatus(exam.Visit_ID, ER_Visit.VisitStatus.IN_EXAMINATION);
            Logger.Info($"Visit {exam.Visit_ID} transitioned to IN_EXAMINATION following saved examination.");
        }
    }
}
