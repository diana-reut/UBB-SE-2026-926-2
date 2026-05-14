using Common.Data.Models;
using Common.Data.Entity.DTOs;

namespace Common.API.Services;

public interface IExaminationService
{
    Task<List<Examination>> GetAllAsync();
    Task<Examination?> GetByIdAsync(int id);
    Task<List<Examination>> GetByVisitIdAsync(int visitId);
    Task<Examination> CreateAsync(Examination examination);
    Task<bool> UpdateAsync(int id, Examination examination);
    Task<bool> DeleteAsync(int id);
    Task<List<ER_Visit>> GetEligibleVisitsAsync();
    Task<List<Examination>> GetPatientHistoryAsync(string patientId);
    Task<ERExaminationSummaryDto?> GetSummaryByVisitIdAsync(int visitId);
}
