using Common.Data.Entity;
using Common.Data.Repository;

namespace Common.API.Services;

public class TransplantsService : ITransplantsService
{
    private readonly ITransplantRepository _repository;

    public TransplantsService(ITransplantRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Transplant>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Transplant?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<Transplant> CreateAsync(Transplant transplant)
    {
        await _repository.AddAsync(transplant);
        return transplant;
    }

    public Task<bool> UpdateAsync(int id, Transplant transplant) =>
        _repository.UpdateAsync(id, transplant);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
