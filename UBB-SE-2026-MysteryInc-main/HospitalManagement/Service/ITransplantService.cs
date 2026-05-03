using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface ITransplantService
{
    public void AssignDonor(int transplantId, int donorId, float finalScore);

    public void CreateWaitlistRequest(int receiverId, string organType);

    public string? GetChronicWarning(int patientId);

    public List<Transplant> GetTopMatchesForDonor(int donorId, string organType);

    public List<TransplantMatch> GetTopMatchesAsDisplayModels(int donorID, string organType);
    public bool IsUrgent(int patientId);
}
