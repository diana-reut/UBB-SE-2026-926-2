using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.ExaminationProxy;

public class ExaminationProxy : ProxyBase, IExaminationProxy
{
    private const string BaseUri = "api/examinations";

    public ExaminationProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<Examination>> GetAllAsync()
    {
        return await GetAsync<List<Examination>>(BaseUri) ?? new List<Examination>();
    }

    public Task<Examination?> GetByIdAsync(int id)
    {
        return GetAsync<Examination>($"{BaseUri}/{id}");
    }

    public async Task<Examination> CreateAsync(Examination examination)
    {
        return await PostAsync<Examination, Examination>(BaseUri, examination) ?? examination;
    }

    public Task UpdateAsync(int id, Examination examination)
    {
        return PutAsync($"{BaseUri}/{id}", examination);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteRequestAsync($"{BaseUri}/{id}");
    }

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
    {
        return await GetAsync<List<Examination>>($"{BaseUri}/visit/{visitId}") ?? new List<Examination>();
    }

    public async Task UpdateNotesAsync(int examId, string notes)
    {
        Examination examination = await GetByIdAsync(examId)
            ?? throw new InvalidOperationException($"Examination {examId} was not found.");

        examination.Notes = notes;
        await UpdateAsync(examId, examination);
    }

    public async Task<List<ER_Visit>> GetEligibleVisitsAsync()
    {
        return await GetAsync<List<ER_Visit>>($"{BaseUri}/eligible-visits") ?? new List<ER_Visit>();
    }

    public async Task<List<Examination>> GetPatientHistoryAsync(string patientId)
    {
        return await GetAsync<List<Examination>>($"{BaseUri}/patient-history/{patientId}") ?? new List<Examination>();
    }

    public Task<ERExaminationSummaryDto?> GetSummaryByVisitIdAsync(int visitId)
    {
        return GetAsync<ERExaminationSummaryDto>($"{BaseUri}/summary/{visitId}");
    }
}
