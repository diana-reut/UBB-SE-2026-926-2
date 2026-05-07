using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IAddictDetectionService
{
    public string BuildPoliceReport(Patient patient);
    public Task<string> BuildPoliceReportAsync(Patient patient);

    public List<Patient> GetAddictCandidates();
    public Task<List<Patient>> GetAddictCandidatesAsync();
}
