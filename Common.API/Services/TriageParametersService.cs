using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TriageParametersService : ITriageParametersService
{
    private readonly ITriageParametersRepository _repository;

    public TriageParametersService(ITriageParametersRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Triage_Parameters>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Triage_Parameters?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters) =>
        _repository.CreateAsync(parameters);

    public Task<bool> UpdateAsync(int id, Triage_Parameters parameters) =>
        _repository.UpdateAsync(id, parameters);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
