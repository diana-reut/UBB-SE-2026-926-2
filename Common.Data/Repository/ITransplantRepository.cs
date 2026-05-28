using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;

namespace Common.Data.Repository;

public interface ITransplantRepository
{
    void Add(Transplant transplant);
    Task AddAsync(Transplant transplant);
    Task<List<Transplant>> GetAllAsync();
    List<Transplant> GetByDonorId(int donorId);
    Task<List<Transplant>> GetByDonorIdAsync(int donorId);
    Transplant? GetById(int id);
    Task<Transplant?> GetByIdAsync(int id);
    List<Transplant> GetByReceiverId(int receiverId);
    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId);
    List<Transplant> GetTopMatches(string organType);
    Task<List<Transplant>> GetTopMatchesAsync(string organType);
    List<Transplant> GetWaitingByOrgan(string organType);
    Task<List<Transplant>> GetWaitingByOrganAsync(string organType);
    void Update(int id, int donorId, float score);
    Task UpdateAsync(int id, int donorId, float score);
    Task<bool> UpdateAsync(int id, Transplant transplant);
    Task<bool> DeleteAsync(int id);
}
