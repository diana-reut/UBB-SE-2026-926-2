using System.Net.Http.Json;
using System.Text.Json;

namespace HospitalManagement.Web.Services;

public class StatisticsApiClient : IStatisticsApiClient
{
    private const string BaseUri = "api/statistics";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    public StatisticsApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("active-vs-archived", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetAgeDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("age-distribution", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetPatientGenderDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("gender-distribution", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetConsultationDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("consultations", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetTopDiagnosesAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("top-diagnoses", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetMostPrescribedMedsAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("top-meds", cancellationToken);
    }

    private async Task<Dictionary<string, int>> GetStatisticsAsync(
        string route,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUri}/{route}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(jsonOptions, cancellationToken) ?? new Dictionary<string, int>();
    }
}
