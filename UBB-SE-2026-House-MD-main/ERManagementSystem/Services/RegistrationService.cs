using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IPatientRepository patientRepository;
        private readonly IERVisitRepository erVisitRepository;

        public RegistrationService(
            IPatientRepository patientRepository,
            IERVisitRepository erVisitRepository)
        {
            this.patientRepository = patientRepository;
            this.erVisitRepository = erVisitRepository;
        }

        public ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint)
        {
            // 1. Validate patient data
            if (!patient.Validate(out var errors))
            {
                throw new InvalidOperationException(
                    "Patient data is invalid:\n" + string.Join("\n", errors));
            }

            // 2. Check if patient already exists — if not, insert
            var existing = patientRepository.GetById(patient.Patient_ID);
            if (existing == null)
            {
                patientRepository.Add(patient);
            }

            // 3. Build the ER_Visit
            var visit = new ER_Visit
            {
                Patient_ID = patient.Patient_ID,
                Chief_Complaint = chiefComplaint,
                Arrival_date_time = DateTime.Now,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            // 4. Validate the visit
            if (!visit.Validate(out var visitErrors))
            {
                throw new InvalidOperationException(
                    "ER Visit data is invalid:\n" + string.Join("\n", visitErrors));
            }

            // 5. Persist the visit
            erVisitRepository.Add(visit);

            return visit;
        }
    }
}
