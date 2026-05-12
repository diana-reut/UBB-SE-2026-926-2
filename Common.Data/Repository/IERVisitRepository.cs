using Common.Data.Models;

namespace Common.Data.Repository;

public interface IERVisitRepository
{
    Task<List<ER_Visit>> GetAllAsync();
    Task<ER_Visit?> GetByIdAsync(int id);
    Task<ER_Visit> CreateAsync(ER_Visit visit);
    Task<bool> UpdateAsync(int id, ER_Visit visit);
    Task<bool> DeleteAsync(int id);
}
