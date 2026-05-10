using Common.Data.Models;

namespace Common.Data.Repository;

public interface ITriageRepository
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<bool> UpdateAsync(int id, Triage triage);
    Task<bool> DeleteAsync(int id);
}
