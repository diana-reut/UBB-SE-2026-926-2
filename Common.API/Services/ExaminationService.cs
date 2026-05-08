using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class ExaminationService : IExaminationService
{
    private readonly IExaminationRepository _repository;

    public ExaminationService(IExaminationRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Examination>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Examination?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<Examination> CreateAsync(Examination examination) =>
        _repository.CreateAsync(examination);

    public Task<bool> UpdateAsync(int id, Examination examination) =>
        _repository.UpdateAsync(id, examination);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
