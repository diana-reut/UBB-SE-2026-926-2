using Common.Data.Entity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.TransplantProxy;

internal class TransplantProxy : ProxyBase, ITransplantProxy
{
    private const string BaseUri = "api/transplants";

    public TransplantProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<Transplant?> GetByIdAsync(int id)
    {
        return await GetAsync<Transplant>($"{BaseUri}/{id}");
    }

    public async Task<List<Transplant>> GetByReceiverIdAsync(int receiverId)
    {
        return await GetAsync<List<Transplant>>($"{BaseUri}/receiver/{receiverId}") ?? [];
    }

    public async Task<List<Transplant>> GetByDonorIdAsync(int donorId)
    {
        return await GetAsync<List<Transplant>>($"{BaseUri}/donor/{donorId}") ?? [];
    }

    public async Task<List<TransplantMatch>> GetTopMatchesForDonorAsync(int donorId, string organType)
    {
        return await GetAsync<List<TransplantMatch>>($"{BaseUri}/matches/donor/{donorId}?organType={Uri.EscapeDataString(organType)}") ?? [];
    }

    public async Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType)
    {
        return await GetAsync<List<TransplantMatch>>($"{BaseUri}/matches/donor/{donorId}?organType={Uri.EscapeDataString(organType)}") ?? [];
    }

    public async Task<bool> IsUrgentAsync(int patientId)
    {
        return await GetAsync<bool>($"{BaseUri}/urgent/{patientId}");
    }

    public async Task<string?> GetChronicWarningAsync(int patientId)
    {
        return await GetAsync<string?>($"{BaseUri}/chronic-warning/{patientId}");
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
    {
        await PostAsync<object, object>($"{BaseUri}/waitlist", new { ReceiverId = receiverId, OrganType = organType });
    }

    public async Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
    {
        await PutAsync($"{BaseUri}/{transplantId}/assign-donor", new { DonorId = donorId, FinalScore = finalScore });
    }
}
