using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class ERVisitService : IERVisitService
{
    private readonly IERVisitRepository _repository;

    public ERVisitService(IERVisitRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ER_Visit>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<ER_Visit?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<ER_Visit> CreateAsync(ER_Visit visit) =>
        _repository.CreateAsync(visit);

    public Task<bool> UpdateAsync(int id, ER_Visit visit) =>
        _repository.UpdateAsync(id, visit);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
