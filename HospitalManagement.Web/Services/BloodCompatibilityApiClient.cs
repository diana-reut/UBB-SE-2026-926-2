using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public class BloodCompatibilityApiClient : IBloodCompatibilityApiClient
{
    private const string BaseUri = "api/bloodcompatibilities";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new () { PropertyNameCaseInsensitive = true };

    public BloodCompatibilityApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{BaseUri}/top-donors",
                new { RecipientId = recipientId },
                jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                throw new InvalidOperationException(error);
            }

            return await response.Content.ReadFromJsonAsync<List<Patient>>(jsonOptions, cancellationToken) ?? new List<Patient>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the blood compatibility API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The blood compatibility API request timed out.");
        }
    }
}
