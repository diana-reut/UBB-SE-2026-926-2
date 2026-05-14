using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;

namespace ERManagementSystem.Proxy.ExaminationProxy;

public interface IExaminationProxy
{
    Task<List<Examination>> GetAllAsync();
    Task<Examination?> GetByIdAsync(int id);
    Task<Examination> CreateAsync(Examination examination);
    Task UpdateAsync(int id, Examination examination);
    Task DeleteAsync(int id);
    Task<List<Examination>> GetByVisitIdAsync(int visitId);
    Task UpdateNotesAsync(int examId, string notes);
    Task<List<ER_Visit>> GetEligibleVisitsAsync();
    Task<List<Examination>> GetPatientHistoryAsync(string patientId);
    Task<ERExaminationSummaryDto?> GetSummaryByVisitIdAsync(int visitId);
}
