using Common.Data.Entity;
using HospitalManagement.Proxy;
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
        return await GetAsync<List<Patient>>(BaseUri) ?? [];
    }

    public async Task<string> BuildPoliceReportAsync(Patient patient)
    {
        return await PostAsync<Patient, string>($"{BaseUri}/police-report", patient) ?? string.Empty;
    }

    public async Task<string> GetChronicConditionsAsync(int patientId)
    {
        return await GetAsync<string>($"{BaseUri}/{patientId}/chronic-conditions") ?? "None reported.";
    }
}