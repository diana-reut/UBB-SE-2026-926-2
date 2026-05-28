using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;
using Common.Data.Integration;

namespace HospitalManagement.Web.Services;

public class PrescriptionApiClient : IPrescriptionApiClient
{
    private const string BaseUri = "api/prescriptions";
    private const int StartupRetryCount = 5;
    private static readonly TimeSpan StartupRetryDelay = TimeSpan.FromMilliseconds(800);

    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PrescriptionApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/latest?n={n}&page={page}", ct),
                cancellationToken);

            return await ReadAsync<List<Prescription>>(response, cancellationToken) ?? new List<Prescription>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the prescription API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The prescription API request timed out or was interrupted.");
        }
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PostAsJsonAsync(BaseUri, filter, jsonOptions, ct),
                cancellationToken);

            return await ReadAsync<List<Prescription>>(response, cancellationToken) ?? new List<Prescription>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the prescription API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The prescription API request timed out or was interrupted.");
        }
    }

    public async Task<Prescription?> GetPrescriptionDetailsAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/{id}", ct),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await ReadAsync<Prescription>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the prescription API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The prescription API request timed out or was interrupted.");
        }
    }

    private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(jsonOptions, cancellationToken);
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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string message = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ArgumentException(message);
        }

        throw new InvalidOperationException(message);
    }
}
