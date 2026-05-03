using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface IExaminationService
    {
        int RequestDoctor(int visitId);
        void SaveExamination(Examination examination);
    }
}
