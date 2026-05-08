using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

public interface IAddictDetectionService
{
    Task<List<Patient>> GetAddictCandidatesAsync();

    Task<string> BuildPoliceReportAsync(Patient patient);

    Task<string> GetChronicConditionsAsync(int patientId);

}
