using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;

namespace Common.API.Services;

public interface IAddictDetectionService
{
    Task<List<Patient>> GetAddictCandidatesAsync();

    Task<string> BuildPoliceReportAsync(int patientId);

    Task<string> GetChronicConditionsAsync(int patientId);

    Task MarkPoliceNotifiedAsync(int patientId);
}
