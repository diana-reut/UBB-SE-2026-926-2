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
        await GetAsync<List<MedicalRecord>>($"{BaseUri}/{historyId}/medical-records", cancellationToken) ?? new List<MedicalRecord>();

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
        await GetAsync<List<string>>($"{BaseUri}/{id}/allergies", cancellationToken) ?? new List<string>();

    public async Task<bool> IsHighRiskPatientAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/{id}/high-risk", cancellationToken);

    public async Task<bool> ExistsAsync(string cnp, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/exists/{cnp}", cancellationToken);

    public async Task<List<Patient>> SearchPatientsAsync(
        SearchPatientsDto dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<SearchPatientsDto, List<Patient>>($"{BaseUri}/search", dto, cancellationToken) ?? new List<Patient>();

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

    public async Task<bool> IsHighRiskAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/{id}/high-risk", cancellationToken);

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(
        int recordId,
        CancellationToken cancellationToken = default) =>
        await GetAsync<RecordExportDataDto>($"{BaseUri}/records/{recordId}/export-data", cancellationToken)
        ?? throw new KeyNotFoundException($"Medical record {recordId} not found.");

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
}
