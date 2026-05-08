using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TriageService : ITriageService
{
    private readonly ITriageRepository _repository;

    public TriageService(ITriageRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Triage>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Triage?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<Triage> CreateAsync(Triage triage) =>
        _repository.CreateAsync(triage);

    public Task<bool> UpdateAsync(int id, Triage triage) =>
        _repository.UpdateAsync(id, triage);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
