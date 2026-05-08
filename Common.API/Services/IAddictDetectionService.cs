using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.API.Services;

public interface IAddictDetectionService
{
    Task<List<Patient>> GetAddictCandidatesAsync();

    Task<string> BuildPoliceReportAsync(Patient patient);

    Task<string> GetChronicConditionsAsync(int patientId);

}
