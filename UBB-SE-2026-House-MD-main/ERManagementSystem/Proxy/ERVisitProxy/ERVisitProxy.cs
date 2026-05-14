using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.ERVisitProxy;

public class ERVisitProxy : ProxyBase, IERVisitProxy
{
    private const string BaseUri = "api/er-visits";

    public ERVisitProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<ER_Visit>> GetAllAsync()
    {
        return await GetAsync<List<ER_Visit>>(BaseUri) ?? new List<ER_Visit>();
    }

    public Task<ER_Visit?> GetByIdAsync(int id)
    {
        return GetAsync<ER_Visit>($"{BaseUri}/{id}");
    }

    public async Task<ER_Visit> CreateAsync(ER_Visit visit)
    {
        return await PostAsync<ER_Visit, ER_Visit>(BaseUri, visit) ?? visit;
    }

    public Task UpdateAsync(int id, ER_Visit visit)
    {
        return PutAsync($"{BaseUri}/{id}", visit);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteAsync($"{BaseUri}/{id}");
    }

    public async Task<List<ER_Visit>> GetByStatusAsync(string status)
    {
        List<ER_Visit> visits = await GetAllAsync();
        return visits
            .Where(visit => string.Equals(visit.Status, status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        ER_Visit visit = await GetByIdAsync(id)
            ?? throw new InvalidOperationException($"ER visit {id} was not found.");

        visit.Status = status;
        await UpdateAsync(id, visit);
    }
}
