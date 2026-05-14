using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.TransferLogProxy;

public class TransferLogProxy : ProxyBase, ITransferLogProxy
{
    private const string BaseUri = "api/transfer-logs";

    public TransferLogProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<Transfer_Log>> GetAllAsync()
    {
        return await GetAsync<List<Transfer_Log>>(BaseUri) ?? new List<Transfer_Log>();
    }

    public Task<Transfer_Log?> GetByIdAsync(int id)
    {
        return GetAsync<Transfer_Log>($"{BaseUri}/{id}");
    }

    public async Task<Transfer_Log> CreateAsync(Transfer_Log transferLog)
    {
        return await PostAsync<Transfer_Log, Transfer_Log>(BaseUri, transferLog) ?? transferLog;
    }

    public Task UpdateAsync(int id, Transfer_Log transferLog)
    {
        return PutAsync($"{BaseUri}/{id}", transferLog);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteRequestAsync($"{BaseUri}/{id}");
    }

    public async Task<List<Transfer_Log>> GetByVisitIdAsync(int visitId)
    {
        return await GetAsync<List<Transfer_Log>>($"{BaseUri}/visit/{visitId}") ?? new List<Transfer_Log>();
    }

    public async Task<List<ERTransferEligibleVisitDto>> GetEligibleVisitsAsync()
    {
        return await GetAsync<List<ERTransferEligibleVisitDto>>($"{BaseUri}/eligible-visits") ?? new List<ERTransferEligibleVisitDto>();
    }
}
