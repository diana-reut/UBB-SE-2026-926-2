using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.StatisticsProxy;

internal class StatisticsProxy : ProxyBase, IStatisticsProxy
{
    private const string BaseUri = "api/statistics";

    public StatisticsProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/active-vs-archived") ?? [];
    }

    public async Task<Dictionary<string, int>> GetAgeDistributionAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/age-distribution") ?? [];
    }

    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/blood-types") ?? [];
    }

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/rh-factor") ?? [];
    }

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/gender-distribution") ?? [];
    }

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/consultations") ?? [];
    }

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-diagnoses") ?? [];
    }

    public async Task<Dictionary<string, int>> GetMostPrescribedMedsAsync()
    {
        return await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-meds") ?? [];
    }
}