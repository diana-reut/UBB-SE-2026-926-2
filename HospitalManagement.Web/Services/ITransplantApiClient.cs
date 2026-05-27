using System;
using System.Collections.Generic;
using System.Text;
using Common.Data.Entity;
namespace HospitalManagement.Web.Services;

public interface ITransplantApiClient
{
    Task<Transplant?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId, CancellationToken cancellationToken);
    Task<List<Transplant>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken);
    Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType, CancellationToken cancellationToken);
    Task<bool> IsUrgentAsync(int patientId, CancellationToken cancellationToken);
    Task<string?> GetChronicWarningAsync(int patientId, CancellationToken cancellationToken);
    Task CreateWaitlistRequestAsync(int receiverId, string organType, CancellationToken cancellationToken);
    Task AssignDonorAsync(int transplantId, int donorId, float finalScore, CancellationToken cancellationToken);
}