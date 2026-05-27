using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TriageService : ITriageService
{
    private readonly ITriageRepository repository;

    public TriageService(ITriageRepository repository)
    {
        this.repository = repository;
    }

    public Task<List<Triage>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<Triage?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public Task<Triage> CreateAsync(Triage triage) =>
        repository.CreateAsync(triage);

    public Task<bool> UpdateAsync(int id, Triage triage) =>
        repository.UpdateAsync(id, triage);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);
}
