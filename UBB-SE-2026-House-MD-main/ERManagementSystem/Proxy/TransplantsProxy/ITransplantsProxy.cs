using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;

namespace ERManagementSystem.Proxy.TransplantsProxy;

public interface ITransplantsProxy
{
    Task<List<Transplant>> GetAllAsync();
    Task<Transplant?> GetByIdAsync(int id);
    Task<Transplant> CreateAsync(Transplant transplant);
    Task UpdateAsync(int id, Transplant transplant);
    Task DeleteAsync(int id);
}
