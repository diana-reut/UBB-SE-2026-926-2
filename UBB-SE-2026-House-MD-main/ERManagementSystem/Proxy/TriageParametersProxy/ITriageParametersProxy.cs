using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.TriageParametersProxy;

public interface ITriageParametersProxy
{
    Task<List<Triage_Parameters>> GetAllAsync();
    Task<Triage_Parameters?> GetByIdAsync(int id);
    Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters);
    Task UpdateAsync(int id, Triage_Parameters parameters);
    Task DeleteAsync(int id);
    Task<Triage_Parameters?> GetByTriageIdAsync(int triageId);
}
