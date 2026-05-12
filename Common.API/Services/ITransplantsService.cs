using Common.Data.Entity;

namespace Common.API.Services;

public interface ITransplantsService
{
    Task<List<Transplant>> GetAllAsync();
    Task<Transplant?> GetByIdAsync(int id);
    Task<Transplant> CreateAsync(Transplant transplant);
    Task<bool> UpdateAsync(int id, Transplant transplant);
    Task<bool> DeleteAsync(int id);
}
