using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.TransplantProxy;

internal interface ITransplantProxy
{
    Task<Transplant?> GetByIdAsync(int id);

    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId);

    Task<List<Transplant>> GetByDonorIdAsync(int donorId);

    Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType);

    Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType);

    Task<bool> IsUrgentAsync(int patientId);

    Task<string?> GetChronicWarningAsync(int patientId);

    Task CreateWaitlistRequestAsync(int receiverId, string organType);

    Task AssignDonorAsync(int transplantId, int donorId, float finalScore);
}
