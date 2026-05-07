using System.Linq;
using Common.Data.Models;
using ERManagementSystem.Helpers;
using Common.Data.Data;
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
        {
            context.Add(triage);
            context.SaveChanges();
            Logger.Info($"[TriageRepository] Created triage {triage.Triage_ID} for visit {triage.Visit_ID}");
            return triage.Triage_ID;
        }

        public Triage? GetByVisitId(int visitId)
        {
            return context.Triages
                .AsNoTracking()
                .FirstOrDefault(t => t.Visit_ID == visitId);
        }

        public void Delete(Triage triage)
        {
            context.Remove(triage);
            context.SaveChanges();
        }
    }
}
