using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.ExaminationProxy;

public class ExaminationProxy : ProxyBase, IExaminationProxy
{
    private const string BaseUri = "api/examinations";

    public ExaminationProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<List<Examination>> GetAllAsync()
    {
        return await GetAsync<List<Examination>>(BaseUri) ?? [];
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
        return DeleteAsync($"{BaseUri}/{id}");
    }

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
    {
        List<Examination> examinations = await GetAllAsync();
        return examinations
            .Where(examination => examination.Visit_ID == visitId)
            .OrderByDescending(examination => examination.Exam_Time)
            .ToList();
    }

    public async Task UpdateNotesAsync(int examId, string notes)
    {
        Examination examination = await GetByIdAsync(examId)
            ?? throw new InvalidOperationException($"Examination {examId} was not found.");

        examination.Notes = notes;
        await UpdateAsync(examId, examination);
    }
}
