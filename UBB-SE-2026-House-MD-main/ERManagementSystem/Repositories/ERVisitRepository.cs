using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Common.Data.Data;
using Microsoft.EntityFrameworkCore;
using Common.Data.Models;

namespace ERManagementSystem.Repositories
{
    public class ERVisitRepository : IERVisitRepository
    {
        private readonly EFHospitalDbContext context;

        public ERVisitRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(ER_Visit visit)
        {
            context.Add(visit);
            context.SaveChanges();
            Logger.Info($"ER Visit created with ID {visit.Visit_ID} for Patient {visit.Patient_ID}.");
        }

        public List<ER_Visit> GetActiveVisits()
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .Where(v => v.Status != ER_Visit.VisitStatus.TRANSFERRED && v.Status != ER_Visit.VisitStatus.CLOSED)
                .ToList();
        }

        public void UpdateStatus(int visitId, string newStatus)
        {
            ER_Visit visit = context.Set<ER_Visit>().First(v => v.Visit_ID == visitId);
            visit.Status = newStatus;
            context.SaveChanges();
        }

        public ER_Visit? GetByVisitId(int visitId)
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .FirstOrDefault(v => v.Visit_ID == visitId);
        }

        public List<ER_Visit> GetByStatus(string status)
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .Where(v => v.Status == status)
                .ToList();
        }

        public List<(ER_Visit visit, Triage triage)> GetActiveVisitsWithTriage()
        {
            return context.Set<ER_Visit>()
                .Join(
                    context.Set<Triage>(),
                    visit => visit.Visit_ID,
                    triage => triage.Visit_ID,
                    (visit, triage) => new { visit, triage })
                .Where(x => x.visit.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                .AsNoTracking()
                .ToList()
                .Select(x => (x.visit, x.triage))
                .ToList();
        }
    }
}
