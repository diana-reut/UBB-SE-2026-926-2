using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public class AllergyApiClient : IAllergyApiClient
{
    private const string BaseUri = "api/allergies";
    private const int StartupRetryCount = 5;
    private static readonly TimeSpan StartupRetryDelay = TimeSpan.FromMilliseconds(800);

    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AllergyApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync(BaseUri, ct),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string message = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
                if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new ArgumentException(message);
                }

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadFromJsonAsync<List<Allergy>>(jsonOptions, cancellationToken) ?? [];
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the allergy API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The allergy API request timed out or was interrupted.");
        }
    }

    private static async Task<HttpResponseMessage> ExecuteWithStartupRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (HttpRequestException) when (attempt < StartupRetryCount && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(StartupRetryDelay, cancellationToken);
            }
            catch (TaskCanceledException) when (attempt < StartupRetryCount && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(StartupRetryDelay, cancellationToken);
            }
        }
    }
}
