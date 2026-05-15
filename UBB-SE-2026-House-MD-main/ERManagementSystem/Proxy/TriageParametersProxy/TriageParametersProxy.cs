using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.TriageParametersProxy;

public class TriageParametersProxy : ProxyBase, ITriageParametersProxy
{
    private const string BaseUri = "api/triage-parameters";

    public TriageParametersProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<Triage_Parameters>> GetAllAsync()
    {
        return await GetAsync<List<Triage_Parameters>>(BaseUri) ?? new List<Triage_Parameters>();
    }

    public Task<Triage_Parameters?> GetByIdAsync(int id)
    {
        return GetAsync<Triage_Parameters>($"{BaseUri}/{id}");
    }

    public async Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters)
    {
        return await PostAsync<Triage_Parameters, Triage_Parameters>(BaseUri, parameters) ?? parameters;
    }

    public Task UpdateAsync(int id, Triage_Parameters parameters)
    {
        return PutAsync($"{BaseUri}/{id}", parameters);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteAsync($"{BaseUri}/{id}");
    }

    public Task<Triage_Parameters?> GetByTriageIdAsync(int triageId)
    {
        return GetByIdAsync(triageId);
    }
}
