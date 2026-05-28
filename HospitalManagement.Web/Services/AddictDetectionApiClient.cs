using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class AddictDetectionApiClient : IAddictDetectionApiClient
{
    private const string BaseUri = "api/addicts";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new () { PropertyNameCaseInsensitive = true };

    public AddictDetectionApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Patient>> GetCandidatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUri}/candidates", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new List<Patient>();
            }

            return await response.Content.ReadFromJsonAsync<List<Patient>>(jsonOptions, cancellationToken) ?? new List<Patient>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the addict detection API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The addict detection API request timed out.");
        }
    }

    public async Task<string> BuildPoliceReportAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new BuildPoliceReportRequestDto { PatientId = patientId };
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{BaseUri}/police-report", dto, jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(error);
            }

            return await response.Content.ReadFromJsonAsync<string>(jsonOptions, cancellationToken) ?? string.Empty;
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the addict detection API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The addict detection API request timed out.");
        }
    }

    public async Task MarkPoliceNotifiedAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsync(
                $"{BaseUri}/{patientId}/notify", null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(error);
            }
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the addict detection API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The addict detection API request timed out.");
        }
    }
}
