using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.AddictDetectionProxy;

public interface IAddictDetectionProxy
{
    Task<List<Patient>> GetAddictCandidatesAsync();

    Task<string> BuildPoliceReportAsync(Patient patient);

    Task<string> GetChronicConditionsAsync(int patientId);
}