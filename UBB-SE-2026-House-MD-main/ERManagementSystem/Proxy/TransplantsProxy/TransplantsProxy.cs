using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.TransplantsProxy;

public class TransplantsProxy : ProxyBase, ITransplantsProxy
{
    private const string BaseUri = "api/transplants";

    public TransplantsProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<Transplant>> GetAllAsync()
    {
        return await GetAsync<List<Transplant>>(BaseUri) ?? new List<Transplant>();
    }

    public Task<Transplant?> GetByIdAsync(int id)
    {
        return GetAsync<Transplant>($"{BaseUri}/{id}");
    }

    public async Task<Transplant> CreateAsync(Transplant transplant)
    {
        return await PostAsync<Transplant, Transplant>(BaseUri, transplant) ?? transplant;
    }

    public Task UpdateAsync(int id, Transplant transplant)
    {
        return PutAsync($"{BaseUri}/{id}", transplant);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteAsync($"{BaseUri}/{id}");
    }
}
