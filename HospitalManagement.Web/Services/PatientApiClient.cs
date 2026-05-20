using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class PatientApiClient : IPatientApiClient
{
    private const string BaseUri = "api/patients";
    private const int StartupRetryCount = 5;
    private static readonly TimeSpan StartupRetryDelay = TimeSpan.FromMilliseconds(800);

    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PatientApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Patient>> SearchPatientsAsync(SearchPatientsDto dto, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PostAsJsonAsync(
                    $"{BaseUri}/search",
                    dto,
                    jsonOptions,
                    ct),
                cancellationToken);

            return await ReadAsync<List<Patient>>(response, cancellationToken) ?? [];
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken)
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

            return await ReadAsync<Patient>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<Patient> GetPatientDetailsAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/{id}/details", ct),
                cancellationToken);
            return await ReadAsync<Patient>(response, cancellationToken)
                ?? throw new InvalidOperationException("Patient details response was empty.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/{id}/allergies", ct),
                cancellationToken);
            return await ReadAsync<List<string>>(response, cancellationToken) ?? [];
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(
        int medicalHistoryId,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/{medicalHistoryId}/medical-records", ct),
                cancellationToken);
            return await ReadAsync<List<MedicalRecord>>(response, cancellationToken) ?? [];
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/records/{recordId}/prescription", ct),
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await ReadAsync<Prescription>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/records/{recordId}/export-data", ct),
                cancellationToken);
            return await ReadAsync<RecordExportDataDto>(response, cancellationToken)
                ?? throw new InvalidOperationException("Record export response was empty.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task<Patient> CreatePatientAsync(CreatePatientDto dto, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PostAsJsonAsync(
                    BaseUri,
                    dto,
                    jsonOptions,
                    ct),
                cancellationToken);

            return await ReadAsync<Patient>(response, cancellationToken)
                ?? throw new InvalidOperationException("Create patient returned an empty response.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryDto dto, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PostAsJsonAsync(
                    $"{BaseUri}/{id}/medical-history",
                    dto,
                    jsonOptions,
                    ct),
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task UpdatePatientAsync(int id, UpdatePatientDto dto, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PutAsJsonAsync(
                    $"{BaseUri}/{id}",
                    dto,
                    jsonOptions,
                    ct),
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task ArchivePatientAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PutAsJsonAsync(
                    $"{BaseUri}/{id}/archive",
                    new { },
                    jsonOptions,
                    ct),
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task DearchivePatientAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PutAsJsonAsync(
                    $"{BaseUri}/{id}/dearchive",
                    new { },
                    jsonOptions,
                    ct),
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
        }
    }

    public async Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedDto dto, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.PutAsJsonAsync(
                    $"{BaseUri}/{id}/archive-deceased",
                    dto,
                    jsonOptions,
                    ct),
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the patient API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The patient API request timed out or was interrupted.");
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
