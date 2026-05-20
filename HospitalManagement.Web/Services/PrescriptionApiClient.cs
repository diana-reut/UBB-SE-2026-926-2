using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;
using Common.Data.Integration;

namespace HospitalManagement.Web.Services;

public class PrescriptionApiClient : IPrescriptionApiClient
{
    private const string BaseUri = "api/prescriptions";
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PrescriptionApiClient(HttpClient http) => _http = http;

    public async Task<Prescription> GetPrescriptionDetailsAsync(int id, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.GetAsync($"{BaseUri}/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ArgumentException($"Prescription with ID {id} does not exist.");
        return await response.Content.ReadFromJsonAsync<Prescription>(_json, cancellationToken)
            ?? throw new InvalidOperationException("Empty response.");
    }

    public async Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.GetAsync($"{BaseUri}/latest?n={n}&page={page}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Prescription>>(_json, cancellationToken) ?? [];
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.PostAsJsonAsync(BaseUri, filter, _json, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Prescription>>(_json, cancellationToken) ?? [];
    }
}