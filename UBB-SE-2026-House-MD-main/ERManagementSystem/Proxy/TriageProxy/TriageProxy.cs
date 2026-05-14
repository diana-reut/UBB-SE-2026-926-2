using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Proxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.TriageParametersProxy;
using ERManagementSystem.Services;

namespace ERManagementSystem.Proxy.TriageProxy;

public class TriageProxy : ProxyBase, ITriageProxy
{
    private const string BaseUri = "api/triages";
    private readonly IERVisitProxy erVisitProxy;
    private readonly ITriageParametersProxy triageParametersProxy;
    private readonly NurseService nurseService;

    public TriageProxy(
        HttpClient httpClient,
        IERVisitProxy erVisitProxy,
        ITriageParametersProxy triageParametersProxy,
        NurseService nurseService)
        : base(httpClient)
    {
        this.erVisitProxy = erVisitProxy;
        this.triageParametersProxy = triageParametersProxy;
        this.nurseService = nurseService;
    }

    public async Task<List<Triage>> GetAllAsync()
    {
        return await GetAsync<List<Triage>>(BaseUri) ?? new List<Triage>();
    }

    public Task<Triage?> GetByIdAsync(int id)
    {
        return GetAsync<Triage>($"{BaseUri}/{id}");
    }

    public async Task<Triage> CreateAsync(Triage triage)
    {
        return await PostAsync<Triage, Triage>(BaseUri, triage) ?? triage;
    }

    public async Task<Triage> CreateTriageAsync(int visitId, Triage_Parameters parameters)
    {
        parameters.ValidateParameters();

        Triage? existingTriage = await GetByVisitIdAsync(visitId);
        if (existingTriage is not null)
        {
            Triage_Parameters? existingParameters =
                await triageParametersProxy.GetByTriageIdAsync(existingTriage.Triage_ID);

            if (existingParameters is not null)
            {
                throw new InvalidOperationException("Triage has already been performed for this visit.");
            }

            await DeleteAsync(existingTriage.Triage_ID);
        }

        int? nurseId = nurseService.RequestAvailableNurse();
        if (nurseId is null)
        {
            throw new InvalidOperationException("No available nurse.");
        }

        Triage triage = new Triage
        {
            Visit_ID = visitId,
            Triage_Level = CalculateTriageLevel(parameters),
            Specialization = DetermineSpecialization(parameters),
            Nurse_ID = nurseId.Value,
            Triage_Time = DateTime.Now
        };

        Triage createdTriage = await CreateAsync(triage);
        parameters.Triage_ID = createdTriage.Triage_ID;
        await triageParametersProxy.CreateAsync(parameters);
        await erVisitProxy.UpdateStatusAsync(visitId, ER_Visit.VisitStatus.TRIAGED);

        return createdTriage;
    }

    public Task UpdateAsync(int id, Triage triage)
    {
        return PutAsync($"{BaseUri}/{id}", triage);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteAsync($"{BaseUri}/{id}");
    }

    public async Task<Triage?> GetByVisitIdAsync(int visitId)
    {
        List<Triage> triages = await GetAllAsync();
        return triages.FirstOrDefault(triage => triage.Visit_ID == visitId);
    }

    public async Task<IReadOnlyList<ER_Visit>> GetVisitsForTriageAsync()
    {
        List<ER_Visit> registeredVisits = await erVisitProxy.GetByStatusAsync(ER_Visit.VisitStatus.REGISTERED);
        List<ER_Visit> triagedVisits = await erVisitProxy.GetByStatusAsync(ER_Visit.VisitStatus.TRIAGED);

        return registeredVisits
            .Concat(triagedVisits)
            .OrderBy(visit => visit.Arrival_date_time)
            .ToList();
    }

    public Task MoveVisitToQueueAsync(int visitId)
    {
        return erVisitProxy.UpdateStatusAsync(visitId, ER_Visit.VisitStatus.WAITING_FOR_ROOM);
    }

    public Task CloseVisitAsync(int visitId)
    {
        return erVisitProxy.UpdateStatusAsync(visitId, ER_Visit.VisitStatus.CLOSED);
    }

    private static int CalculateTriageLevel(Triage_Parameters parameters)
    {
        if (parameters.Consciousness == 3 ||
            parameters.Breathing == 3 ||
            parameters.Injury_Type == 3 ||
            parameters.Bleeding == 3)
        {
            return 1;
        }

        int severityScore =
            (parameters.Consciousness * 3) +
            (parameters.Breathing * 3) +
            (parameters.Bleeding * 2) +
            (parameters.Injury_Type * 2) +
            parameters.Pain_Level;

        if (severityScore >= 20)
        {
            return 2;
        }

        if (severityScore >= 16)
        {
            return 3;
        }

        if (severityScore >= 12)
        {
            return 4;
        }

        return 5;
    }

    private static string DetermineSpecialization(Triage_Parameters parameters)
    {
        if (parameters.Bleeding == 3 || parameters.Injury_Type == 3)
        {
            return "General Surgery";
        }

        if (parameters.Injury_Type == 2)
        {
            return "Orthopedics";
        }

        if (parameters.Breathing == 2)
        {
            return "Pulmonology";
        }

        if (parameters.Consciousness == 2 || parameters.Consciousness == 3)
        {
            return "Neurology";
        }

        return "Emergency Medicine";
    }
}
