using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TriageParametersService : ITriageParametersService
{
    private readonly ITriageParametersRepository repository;

    public TriageParametersService(ITriageParametersRepository repository)
    {
        this.repository = repository;
    }

    public Task<List<Triage_Parameters>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<Triage_Parameters?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters) =>
        repository.CreateAsync(parameters);

    public Task<bool> UpdateAsync(int id, Triage_Parameters parameters) =>
        repository.UpdateAsync(id, parameters);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);
}
