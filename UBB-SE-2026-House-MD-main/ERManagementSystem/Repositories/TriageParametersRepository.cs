using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ERManagementSystem.Repositories
{
    public class TriageParametersRepository : ITriageParametersRepository
    {
        private readonly EFHospitalDbContext context;

        public TriageParametersRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(Triage_Parameters parameters)
            => AddAsync(parameters).GetAwaiter().GetResult();

        public async Task AddAsync(Triage_Parameters parameters)
        {
            await context.AddAsync(parameters);
            await context.SaveChangesAsync();
            Logger.Info($"[TriageParametersRepository] Parameters saved for triage {parameters.Triage_ID}");
        }

        public Triage_Parameters? GetByTriageId(int triageId)
            => GetByTriageIdAsync(triageId).GetAwaiter().GetResult();

        public Task<Triage_Parameters?> GetByTriageIdAsync(int triageId)
        {
            return context.TriageParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.Triage_ID == triageId);
        }

        public void Delete(Triage_Parameters parameters)
            => DeleteAsync(parameters).GetAwaiter().GetResult();

        public async Task DeleteAsync(Triage_Parameters parameters)
        {
            context.Remove(parameters);
            await context.SaveChangesAsync();
        }
    }
}
