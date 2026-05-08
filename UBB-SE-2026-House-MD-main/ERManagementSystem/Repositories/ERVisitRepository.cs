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
    public class ERVisitRepository : IERVisitRepository
    {
        private readonly EFHospitalDbContext context;

        public ERVisitRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(ER_Visit visit)
            => AddAsync(visit).GetAwaiter().GetResult();

        public async Task AddAsync(ER_Visit visit)
        {
            await context.AddAsync(visit);
            await context.SaveChangesAsync();
            Logger.Info($"ER Visit created with ID {visit.Visit_ID} for Patient {visit.Patient_ID}.");
        }

        public List<ER_Visit> GetActiveVisits()
            => GetActiveVisitsAsync().GetAwaiter().GetResult();

        public Task<List<ER_Visit>> GetActiveVisitsAsync()
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .Where(v => v.Status != ER_Visit.VisitStatus.TRANSFERRED && v.Status != ER_Visit.VisitStatus.CLOSED)
                .ToListAsync();
        }

        public void UpdateStatus(int visitId, string newStatus)
            => UpdateStatusAsync(visitId, newStatus).GetAwaiter().GetResult();

        public async Task UpdateStatusAsync(int visitId, string newStatus)
        {
            ER_Visit visit = await context.Set<ER_Visit>().FirstAsync(v => v.Visit_ID == visitId);
            visit.Status = newStatus;
            await context.SaveChangesAsync();
        }

        public ER_Visit? GetByVisitId(int visitId)
            => GetByVisitIdAsync(visitId).GetAwaiter().GetResult();

        public Task<ER_Visit?> GetByVisitIdAsync(int visitId)
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Visit_ID == visitId);
        }

        public List<ER_Visit> GetByStatus(string status)
            => GetByStatusAsync(status).GetAwaiter().GetResult();

        public Task<List<ER_Visit>> GetByStatusAsync(string status)
        {
            return context.Set<ER_Visit>()
                .AsNoTracking()
                .Where(v => v.Status == status)
                .ToListAsync();
        }

        public List<(ER_Visit visit, Triage triage)> GetActiveVisitsWithTriage()
            => GetActiveVisitsWithTriageAsync().GetAwaiter().GetResult();

        public async Task<List<(ER_Visit visit, Triage triage)>> GetActiveVisitsWithTriageAsync()
        {
            return (await context.Set<ER_Visit>()
                .Join(
                    context.Set<Triage>(),
                    visit => visit.Visit_ID,
                    triage => triage.Visit_ID,
                    (visit, triage) => new { visit, triage })
                .Where(x => x.visit.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                .AsNoTracking()
                .ToListAsync())
                .Select(x => (x.visit, x.triage))
                .ToList();
        }
    }
}
