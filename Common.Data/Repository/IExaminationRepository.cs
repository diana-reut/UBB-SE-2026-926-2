using Common.Data.Models;

namespace Common.Data.Repository;

public interface IExaminationRepository
{
    Task<List<Examination>> GetAllAsync();
    Task<Examination?> GetByIdAsync(int id);
    Task<Examination> CreateAsync(Examination examination);
    Task<bool> UpdateAsync(int id, Examination examination);
    Task<bool> DeleteAsync(int id);
}
