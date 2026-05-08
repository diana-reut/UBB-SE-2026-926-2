using System.Threading.Tasks;
using Common.Data.Models;

namespace ERManagementSystem.Services
{
    public interface IExaminationService
    {
        int RequestDoctor(int visitId);
        Task<int> RequestDoctorAsync(int visitId);
        void SaveExamination(Examination examination);
        Task SaveExaminationAsync(Examination examination);
    }
}
