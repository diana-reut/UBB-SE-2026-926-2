using Common.Data.Models;

namespace Common.API.Services;

public interface ITriageParametersService
{
    Task<List<Triage_Parameters>> GetAllAsync();
    Task<Triage_Parameters?> GetByIdAsync(int id);
    Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters);
    Task<bool> UpdateAsync(int id, Triage_Parameters parameters);
    Task<bool> DeleteAsync(int id);
}
