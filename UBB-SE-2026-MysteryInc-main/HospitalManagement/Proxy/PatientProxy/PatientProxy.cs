using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Common.Data.Entity.DTOs;
using Common.Data.Entity;
using Common.Data.Integration;

namespace HospitalManagement.Proxy.PatientProxy;

internal class PatientProxy : ProxyBase, IPatientProxy
{
    private const string BaseUri = "api/patients";

    public PatientProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await GetAsync<Patient>($"{BaseUri}/{id}");
    }

    public async Task<Patient> GetPatientDetailsAsync(int id)
    {
        return await GetAsync<Patient>($"{BaseUri}/{id}/details") ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");
    }

    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int id)
    {
        return await GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history");
    }

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId)
    {
        return await GetAsync<List<MedicalRecord>>($"{BaseUri}/{historyId}/medical-records") ?? [];
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int id)
    {
        return await GetAsync<List<string>>($"{BaseUri}/{id}/allergies") ?? [];
    }

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId)
    {
        return await GetAsync<Prescription>($"{BaseUri}/records/{recordId}/prescription");
    }

    public async Task<bool> IsHighRiskPatientAsync(int id)
    {
        return await GetAsync<bool>($"{BaseUri}/{id}/high-risk");
    }

    public async Task<bool> ExistsAsync(string cnp)
    {
        return await GetAsync<bool>($"{BaseUri}/exists/{cnp}");
    }

    public async Task<List<Patient>> SearchPatientsAsync(SearchPatientsDto dto)
    {
        return await PostAsync<SearchPatientsDto, List<Patient>>($"{BaseUri}/search", dto) ?? [];
    }

    public async Task<Patient> CreatePatientAsync(CreatePatientDto dto)
    {
        return await PostAsync<CreatePatientDto, Patient>(BaseUri, dto) ?? throw new InvalidOperationException("Failed to create patient: no response from server.");
    }

    public async Task UpdatePatientAsync(int id, UpdatePatientDto dto)
    {
        await PutAsync($"{BaseUri}/{id}", dto);
    }

    public async Task ArchivePatientAsync(int id)
    {
        await PutAsync<object>($"{BaseUri}/{id}/archive", new { });
    }

    public async Task DearchivePatientAsync(int id)
    {
        await PutAsync<object>($"{BaseUri}/{id}/dearchive", new { });
    }

    public async Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedDto dto)
    {
        await PutAsync($"{BaseUri}/{id}/archive-deceased", dto);
    }

    public async Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryDto dto)
    {
        await PostAsync<CreateMedicalHistoryDto, object>($"{BaseUri}/{id}/medical-history", dto);
    }

    public async Task DeletePatientAsync(int id)
    {
        await DeleteAsync($"{BaseUri}/{id}");
    }
}
