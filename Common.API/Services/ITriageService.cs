using Common.Data.Models;

namespace Common.API.Services;

public interface ITriageService
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<bool> UpdateAsync(int id, Triage triage);
    Task<bool> DeleteAsync(int id);
}
