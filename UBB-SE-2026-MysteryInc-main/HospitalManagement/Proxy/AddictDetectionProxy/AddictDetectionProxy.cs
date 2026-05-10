using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.AddictDetectionProxy;

internal class AddictDetectionProxy : ProxyBase, IAddictDetectionProxy
{
    private const string BaseUri = "api/addicts";

    public AddictDetectionProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<List<Patient>> GetAddictCandidatesAsync()
    {
        return await GetAsync<List<Patient>>($"{BaseUri}/candidates") ?? [];
    }

    public async Task<string> BuildPoliceReportAsync(Patient patient)
    {
        var dto = new BuildPoliceReportRequestDto
        {
            Patient = new Patient
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Cnp = patient.Cnp,
                PhoneNo = patient.PhoneNo,
            }
        };
        return await PostAsync<BuildPoliceReportRequestDto, string>($"{BaseUri}/police-report", dto) ?? string.Empty;
    }

    public async Task<string> GetChronicConditionsAsync(int patientId)
    {
        return await GetAsync<string>($"{BaseUri}/{patientId}/chronic-conditions") ?? "None reported.";
    }
}