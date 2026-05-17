using Common.Data.Entity;

namespace Common.API.Services;

public interface ITransplantService
{
    Task<List<Transplant>> GetAllAsync();
    Task<Transplant?> GetByIdAsync(int id);
    Task<Transplant> CreateAsync(Transplant transplant);
    Task<bool> UpdateAsync(int id, Transplant transplant);
    Task<bool> DeleteAsync(int id);
    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId);
    Task<List<Transplant>> GetByDonorIdAsync(int donorId);

    Task CreateWaitlistRequestAsync(int receiverId, string organType);
    Task AssignDonorAsync(int transplantId, int donorId, float finalScore);

    Task<List<Transplant>> GetTopMatchesForDonorAsync(int donorId, string organType);
    Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType);

    Task<bool> IsUrgentAsync(int patientId);
    Task<string?> GetChronicWarningAsync(int patientId);
}