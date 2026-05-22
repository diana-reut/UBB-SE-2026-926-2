using System.Net;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class PatientApiClient : HospitalApiClientBase, IPatientApiClient
{
    private const string BaseUri = "api/patients";

    public PatientApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Patient>($"{BaseUri}/{id}", cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<Patient> GetPatientDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<Patient>($"{BaseUri}/{id}/details", cancellationToken)
        ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

    public Task<MedicalHistory?> GetMedicalHistoryAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history", cancellationToken);

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<MedicalRecord>>($"{BaseUri}/{historyId}/medical-records", cancellationToken) ?? [];

    public async Task<int> CreateMedicalRecordAsync(
        int patientId,
        CreateMedicalRecordDto dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<CreateMedicalRecordDto, int>($"{BaseUri}/{patientId}/medical-records", dto, cancellationToken);

    public Task CreatePrescriptionForRecordAsync(
        int recordId,
        CreatePrescriptionDto dto,
        CancellationToken cancellationToken = default) =>
        PostAsync($"{BaseUri}/records/{recordId}/prescription", dto, cancellationToken);

    public async Task<List<string>> GetPatientAllergiesAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<List<string>>($"{BaseUri}/{id}/allergies", cancellationToken) ?? [];

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(
        int recordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Prescription>($"{BaseUri}/records/{recordId}/prescription", cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message.Contains(((int)HttpStatusCode.NotFound).ToString(), StringComparison.Ordinal))
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(
        int recordId,
        CancellationToken cancellationToken = default) =>
        await GetAsync<RecordExportDataDto>($"{BaseUri}/records/{recordId}/export-data", cancellationToken)
        ?? throw new KeyNotFoundException($"Medical record {recordId} not found.");

    public async Task<bool> IsHighRiskPatientAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/{id}/high-risk", cancellationToken);

    public async Task<bool> ExistsAsync(string cnp, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/exists/{cnp}", cancellationToken);

    public async Task<List<Patient>> SearchPatientsAsync(
        SearchPatientsDto dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<SearchPatientsDto, List<Patient>>($"{BaseUri}/search", dto, cancellationToken) ?? [];

    public async Task<Patient> CreatePatientAsync(
        CreatePatientDto dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<CreatePatientDto, Patient>(BaseUri, dto, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create patient: no response from server.");

    public Task UpdatePatientAsync(int id, UpdatePatientDto dto, CancellationToken cancellationToken = default) =>
        PutAsync($"{BaseUri}/{id}", dto, cancellationToken);

    public Task ArchivePatientAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync<object>($"{BaseUri}/{id}/archive", new { }, cancellationToken);

    public Task DearchivePatientAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync<object>($"{BaseUri}/{id}/dearchive", new { }, cancellationToken);

    public Task ArchiveAsDeceasedAsync(
        int id,
        ArchiveAsDeceasedDto dto,
        CancellationToken cancellationToken = default) =>
        PutAsync($"{BaseUri}/{id}/archive-deceased", dto, cancellationToken);

    public Task CreateMedicalHistoryAsync(
        int id,
        CreateMedicalHistoryDto dto,
        CancellationToken cancellationToken = default) =>
        PostAsync($"{BaseUri}/{id}/medical-history", dto, cancellationToken);

    public Task DeletePatientAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"{BaseUri}/{id}", cancellationToken);
        throw new InvalidOperationException(message);
    }

    public async Task<bool> IsHighRiskAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/{id}/high-risk", ct),
                cancellationToken);
            return await ReadAsync<bool>(response, cancellationToken);
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

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/records/{recordId}/export-data", ct),
                cancellationToken);
            return await ReadAsync<RecordExportDataDto>(response, cancellationToken)
                ?? throw new InvalidOperationException("Export data response was empty.");
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

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await ExecuteWithStartupRetryAsync(
                ct => httpClient.GetAsync($"{BaseUri}/records/{recordId}/prescription", ct),
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
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
}
