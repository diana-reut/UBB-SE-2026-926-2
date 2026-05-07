using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IAddictDetectionService
{
    public Task<string> BuildPoliceReportAsync(Patient patient);

    public Task<List<Patient>> GetAddictCandidatesAsync();
}
