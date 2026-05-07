using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ERManagementSystem.Repositories
{
    public class TriageRepository : ITriageRepository
    {
        private readonly EFHospitalDbContext context;

        public TriageRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public int Add(Triage triage)
            => AddAsync(triage).GetAwaiter().GetResult();

        public async Task<int> AddAsync(Triage triage)
        {
            await context.AddAsync(triage);
            await context.SaveChangesAsync();
            Logger.Info($"[TriageRepository] Created triage {triage.Triage_ID} for visit {triage.Visit_ID}");
            return triage.Triage_ID;
        }

        public Triage? GetByVisitId(int visitId)
            => GetByVisitIdAsync(visitId).GetAwaiter().GetResult();

        public Task<Triage?> GetByVisitIdAsync(int visitId)
        {
            return context.Triages
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Visit_ID == visitId);
        }

        public void Delete(Triage triage)
            => DeleteAsync(triage).GetAwaiter().GetResult();

        public async Task DeleteAsync(Triage triage)
        {
            context.Remove(triage);
            await context.SaveChangesAsync();
        }
    }
}
