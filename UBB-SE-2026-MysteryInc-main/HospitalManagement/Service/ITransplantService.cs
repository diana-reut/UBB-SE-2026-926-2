using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface ITransplantService
{
    public void AssignDonor(int transplantId, int donorId, float finalScore);
    public Task AssignDonorAsync(int transplantId, int donorId, float finalScore);

    public void CreateWaitlistRequest(int receiverId, string organType);
    public Task CreateWaitlistRequestAsync(int receiverId, string organType);

    public string? GetChronicWarning(int patientId);
    public Task<string?> GetChronicWarningAsync(int patientId);

    public List<Transplant> GetTopMatchesForDonor(int donorId, string organType);
    public Task<List<Transplant>> GetTopMatchesForDonorAsync(int donorId, string organType);

    public List<TransplantMatch> GetTopMatchesAsDisplayModels(int donorID, string organType);
    public Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorID, string organType);

    public bool IsUrgent(int patientId);
    public Task<bool> IsUrgentAsync(int patientId);
}
