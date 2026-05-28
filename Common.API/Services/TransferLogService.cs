using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TransferLogService : ITransferLogService
{
    private readonly ITransferLogRepository repository;
    private readonly IERVisitRepository erVisitRepository;
    private readonly IPatientRepository patientRepository;

    public TransferLogService(
        ITransferLogRepository repository,
        IERVisitRepository erVisitRepository,
        IPatientRepository patientRepository)
    {
        this.repository = repository;
        this.erVisitRepository = erVisitRepository;
        this.patientRepository = patientRepository;
    }

    public Task<List<Transfer_Log>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<Transfer_Log?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public Task<Transfer_Log> CreateAsync(Transfer_Log transferLog) =>
        repository.CreateAsync(transferLog);

    public Task<bool> UpdateAsync(int id, Transfer_Log transferLog) =>
        repository.UpdateAsync(id, transferLog);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);

    public async Task<List<Transfer_Log>> GetByVisitIdAsync(int visitId)
    {
        return (await repository.GetAllAsync())
            .Where(log => log.Visit_ID == visitId)
            .OrderByDescending(log => log.Transfer_Time)
            .ToList();
    }

    public async Task<List<ERTransferEligibleVisitDto>> GetEligibleVisitsAsync()
    {
        List<ER_Visit> visits = (await erVisitRepository.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ER_Visit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase))
            .OrderBy(visit => visit.Arrival_date_time)
            .ToList();

        List<ERTransferEligibleVisitDto> result = new ();
        foreach (ER_Visit visit in visits)
        {
            Patient? patient = (await patientRepository.SearchAsync(new PatientFilter { CNP = visit.Patient_ID }))
                .FirstOrDefault();

            result.Add(new ERTransferEligibleVisitDto
            {
                Visit_ID = visit.Visit_ID,
                Chief_Complaint = visit.Chief_Complaint,
                Status = visit.Status,
                PatientName = patient == null ? visit.Patient_ID : $"{patient.First_Name} {patient.Last_Name}",
                Transferred = patient?.Transferred ?? false
            });
        }

        return result;
    }
}
