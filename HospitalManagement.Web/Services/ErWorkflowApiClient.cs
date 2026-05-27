using Common.Data.Entity.DTOs;
using Common.Data.Models;

namespace HospitalManagement.Web.Services;

public class ErWorkflowApiClient : HospitalApiClientBase, IErWorkflowApiClient
{
    private const string VisitsBaseUri = "api/er-visits";
    private const string RoomsBaseUri = "api/er-rooms";
    private const string TriagesBaseUri = "api/triages";
    private const string TriageParametersBaseUri = "api/triage-parameters";
    private const string ExaminationsBaseUri = "api/examinations";
    private const string TransferLogsBaseUri = "api/transfer-logs";

    public ErWorkflowApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<List<ER_Visit>> GetVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ER_Visit>>(VisitsBaseUri, cancellationToken) ?? new List<ER_Visit>();

    public async Task<List<ER_Visit>> GetVisitsByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        (await GetVisitsAsync(cancellationToken))
            .Where(visit => string.Equals(visit.Status, status, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public Task<ER_Visit?> GetVisitAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<ER_Visit>($"{VisitsBaseUri}/{id}", cancellationToken);

    public async Task<ER_Visit> CreateVisitAsync(ER_Visit visit, CancellationToken cancellationToken = default) =>
        await PostAsync<ER_Visit, ER_Visit>(VisitsBaseUri, visit, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create ER visit: no response from server.");

    public Task UpdateVisitAsync(int id, ER_Visit visit, CancellationToken cancellationToken = default) =>
        PutAsync($"{VisitsBaseUri}/{id}", visit, cancellationToken);

    public async Task UpdateVisitStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        ER_Visit visit = await GetVisitAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"ER visit {id} was not found.");

        visit.Status = status;
        await UpdateVisitAsync(id, visit, cancellationToken);
    }

    public async Task<bool> AutoAssignHighestPriorityRoomAsync(CancellationToken cancellationToken = default) =>
        await PostAsync<object, bool>($"{VisitsBaseUri}/auto-assign-room", new { }, cancellationToken);

    public Task AssignRoomAsync(int visitId, int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/assign-room/{roomId}", new { }, cancellationToken);

    public Task TransferVisitAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/transfer", new { }, cancellationToken);

    public Task RetryTransferAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/retry-transfer", new { }, cancellationToken);

    public Task CloseVisitAsync(int visitId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{VisitsBaseUri}/{visitId}/close", new { }, cancellationToken);

    public async Task<List<ER_Room>> GetRoomsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ER_Room>>(RoomsBaseUri, cancellationToken) ?? new List<ER_Room>();

    public async Task<List<ER_Room>> GetRoomsByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        await GetAsync<List<ER_Room>>($"{RoomsBaseUri}/status/{status}", cancellationToken) ?? new List<ER_Room>();

    public async Task<ERRoomVisitDetailsDto?> GetRoomVisitDetailsAsync(int roomId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<ERRoomVisitDetailsDto>($"{RoomsBaseUri}/{roomId}/visit-details", cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("404", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
    }

    public Task MarkRoomAsCleaningAsync(int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{RoomsBaseUri}/{roomId}/mark-cleaning", new { }, cancellationToken);

    public Task MarkRoomAsAvailableAsync(int roomId, CancellationToken cancellationToken = default) =>
        PostAsync<object>($"{RoomsBaseUri}/{roomId}/mark-available", new { }, cancellationToken);

    public async Task<List<Triage>> GetTriagesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Triage>>(TriagesBaseUri, cancellationToken) ?? new List<Triage>();

    public async Task<Triage?> GetTriageByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        (await GetTriagesAsync(cancellationToken)).FirstOrDefault(triage => triage.Visit_ID == visitId);

    public async Task<Triage> CreateTriageAsync(Triage triage, CancellationToken cancellationToken = default) =>
        await PostAsync<Triage, Triage>(TriagesBaseUri, triage, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create triage: no response from server.");

    public async Task<PerformTriageResponseDto> PerformTriageAsync(
        PerformTriageRequestDto request,
        CancellationToken cancellationToken = default) =>
        await PostAsync<PerformTriageRequestDto, PerformTriageResponseDto>(
            $"{TriagesBaseUri}/perform",
            request,
            cancellationToken)
        ?? throw new InvalidOperationException("Failed to perform triage: no response from server.");

    public async Task<List<Triage_Parameters>> GetTriageParametersAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Triage_Parameters>>(TriageParametersBaseUri, cancellationToken) ?? new List<Triage_Parameters>();

    public async Task<Triage_Parameters?> GetTriageParametersByTriageIdAsync(int triageId, CancellationToken cancellationToken = default) =>
        (await GetTriageParametersAsync(cancellationToken)).FirstOrDefault(parameters => parameters.Triage_ID == triageId);

    public async Task<Triage_Parameters> CreateTriageParametersAsync(
        Triage_Parameters parameters,
        CancellationToken cancellationToken = default) =>
        await PostAsync<Triage_Parameters, Triage_Parameters>(TriageParametersBaseUri, parameters, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create triage parameters: no response from server.");

    public async Task<List<ER_Visit>> GetEligibleExaminationVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ER_Visit>>($"{ExaminationsBaseUri}/eligible-visits", cancellationToken) ?? new List<ER_Visit>();

    public async Task<List<Examination>> GetExaminationsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Examination>>($"{ExaminationsBaseUri}/visit/{visitId}", cancellationToken) ?? new List<Examination>();

    public async Task<List<Examination>> GetPatientExaminationHistoryAsync(string patientId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Examination>>($"{ExaminationsBaseUri}/patient-history/{patientId}", cancellationToken) ?? new List<Examination>();

    public Task<ERExaminationSummaryDto?> GetExaminationSummaryAsync(int visitId, CancellationToken cancellationToken = default) =>
        GetAsync<ERExaminationSummaryDto>($"{ExaminationsBaseUri}/summary/{visitId}", cancellationToken);

    public async Task<Examination> CreateExaminationAsync(Examination examination, CancellationToken cancellationToken = default) =>
        await PostAsync<Examination, Examination>(ExaminationsBaseUri, examination, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create examination: no response from server.");

    public Task UpdateExaminationAsync(int examId, Examination examination, CancellationToken cancellationToken = default) =>
        PutAsync($"{ExaminationsBaseUri}/{examId}", examination, cancellationToken);

    public async Task<List<ERTransferEligibleVisitDto>> GetEligibleTransferVisitsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ERTransferEligibleVisitDto>>($"{TransferLogsBaseUri}/eligible-visits", cancellationToken) ?? new List<ERTransferEligibleVisitDto>();

    public async Task<List<Transfer_Log>> GetTransferLogsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Transfer_Log>>($"{TransferLogsBaseUri}/visit/{visitId}", cancellationToken) ?? new List<Transfer_Log>();
}
