using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IExaminationRepository
    {
        void Add(Examination exam);
        Task AddAsync(Examination exam);
        List<Examination> GetByPatientId(string patientId);
        Task<List<Examination>> GetByPatientIdAsync(string patientId);
        void UpdateNotes(int examId, string notes);
        Task UpdateNotesAsync(int examId, string notes);
        ExaminationSummaryDTO? GetExaminationSummary(int examId);
        Task<ExaminationSummaryDTO?> GetExaminationSummaryAsync(int examId);
        int GetFirstRoomId();
        Task<int> GetFirstRoomIdAsync();
    }
}
