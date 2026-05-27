using System.Net.Http.Json;
using System.Text.Json;

namespace HospitalManagement.Web.Services;

public class GhostApiClient : IGhostApiClient
{
    private const string BaseUri = "api/ghost";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    public GhostApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<GhostStatusDto> ReportSightingAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsync($"{BaseUri}/sighting", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GhostStatusDto>(jsonOptions, cancellationToken)
            ?? new GhostStatusDto(false, 0);
    }

    public async Task<GhostStatusDto> GetExorcismStatusAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUri}/exorcism-status", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GhostStatusDto>(jsonOptions, cancellationToken)
            ?? new GhostStatusDto(false, 0);
    }
}
