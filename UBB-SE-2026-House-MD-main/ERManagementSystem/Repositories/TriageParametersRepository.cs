using System.Linq;
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
        {
            context.Add(parameters);
            context.SaveChanges();
            Logger.Info($"[TriageParametersRepository] Parameters saved for triage {parameters.Triage_ID}");
        }

        public Triage_Parameters? GetByTriageId(int triageId)
        {
            return context.TriageParameters
                .AsNoTracking()
                .FirstOrDefault(tp => tp.Triage_ID == triageId);
        }

        public void Delete(Triage_Parameters parameters)
        {
            context.Remove(parameters);
            context.SaveChanges();
        }
    }
}
