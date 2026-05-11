using Common.Data.Entity;
using Common.Data.Integration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.PrescriptionProxy;

internal class PrescriptionProxy : ProxyBase, IPrescriptionProxy
{
    private const string BaseUri = "api/prescriptions";

    public PrescriptionProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page)
    {
        return await GetAsync<List<Prescription>>($"{BaseUri}/latest?n={n}&page={page}") ?? [];
    }

    public async Task<Prescription> GetPrescriptionDetailsAsync(int id)
    {
        return await GetAsync<Prescription>($"{BaseUri}/{id}") ?? throw new ArgumentException($"Prescription with ID {id} does not exist.");
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter)
    {
        return await PostAsync<PrescriptionFilter, List<Prescription>>(BaseUri, filter) ?? [];
    }
}