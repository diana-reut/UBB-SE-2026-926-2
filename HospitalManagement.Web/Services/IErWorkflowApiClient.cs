using Common.Data.Entity.DTOs;
using Common.Data.Models;

namespace HospitalManagement.Web.Services;

public interface IErWorkflowApiClient
{
    Task<List<ER_Visit>> GetVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<ER_Visit>> GetVisitsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<ER_Visit?> GetVisitAsync(int id, CancellationToken cancellationToken = default);
    Task<ER_Visit> CreateVisitAsync(ER_Visit visit, CancellationToken cancellationToken = default);
    Task UpdateVisitAsync(int id, ER_Visit visit, CancellationToken cancellationToken = default);
    Task UpdateVisitStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task<bool> AutoAssignHighestPriorityRoomAsync(CancellationToken cancellationToken = default);
    Task AssignRoomAsync(int visitId, int roomId, CancellationToken cancellationToken = default);
    Task TransferVisitAsync(int visitId, CancellationToken cancellationToken = default);
    Task RetryTransferAsync(int visitId, CancellationToken cancellationToken = default);
    Task CloseVisitAsync(int visitId, CancellationToken cancellationToken = default);

    Task<List<ER_Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
    Task<List<ER_Room>> GetRoomsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<ERRoomVisitDetailsDto?> GetRoomVisitDetailsAsync(int roomId, CancellationToken cancellationToken = default);
    Task MarkRoomAsCleaningAsync(int roomId, CancellationToken cancellationToken = default);
    Task MarkRoomAsAvailableAsync(int roomId, CancellationToken cancellationToken = default);

    Task<List<Triage>> GetTriagesAsync(CancellationToken cancellationToken = default);
    Task<Triage?> GetTriageByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
    Task<Triage> CreateTriageAsync(Triage triage, CancellationToken cancellationToken = default);
    Task<PerformTriageResponseDto> PerformTriageAsync(PerformTriageRequestDto request, CancellationToken cancellationToken = default);

    Task<List<Triage_Parameters>> GetTriageParametersAsync(CancellationToken cancellationToken = default);
    Task<Triage_Parameters?> GetTriageParametersByTriageIdAsync(int triageId, CancellationToken cancellationToken = default);
    Task<Triage_Parameters> CreateTriageParametersAsync(Triage_Parameters parameters, CancellationToken cancellationToken = default);

    Task<List<ER_Visit>> GetEligibleExaminationVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<Examination>> GetExaminationsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
    Task<List<Examination>> GetPatientExaminationHistoryAsync(string patientId, CancellationToken cancellationToken = default);
    Task<ERExaminationSummaryDto?> GetExaminationSummaryAsync(int visitId, CancellationToken cancellationToken = default);
    Task<Examination> CreateExaminationAsync(Examination examination, CancellationToken cancellationToken = default);
    Task UpdateExaminationAsync(int examId, Examination examination, CancellationToken cancellationToken = default);

    Task<List<ERTransferEligibleVisitDto>> GetEligibleTransferVisitsAsync(CancellationToken cancellationToken = default);
    Task<List<Transfer_Log>> GetTransferLogsByVisitIdAsync(int visitId, CancellationToken cancellationToken = default);
}
