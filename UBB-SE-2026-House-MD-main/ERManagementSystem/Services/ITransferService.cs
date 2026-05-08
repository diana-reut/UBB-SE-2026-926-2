using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Models;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface ITransferService
    {
        Transfer_Log SendPatientData(int visitId);
        Task<Transfer_Log> SendPatientDataAsync(int visitId);
        void LogTransfer(int visitId, string status);
        Task LogTransferAsync(int visitId, string status);
        List<Transfer_Log> GetLogs(int visitId);
        Task<List<Transfer_Log>> GetLogsAsync(int visitId);
        Transfer_Log RetryTransfer(int visitId);
        Task<Transfer_Log> RetryTransferAsync(int visitId);
        void MarkPatientAsTransferred(int visitId);
        Task MarkPatientAsTransferredAsync(int visitId);
        void TransitionVisitToTransferred(int visitId);
        Task TransitionVisitToTransferredAsync(int visitId);
        void CloseVisit(int visitId);
        Task CloseVisitAsync(int visitId);
        List<TransferEligibleVisit> GetEligibleVisitsForTransfer();
        Task<List<TransferEligibleVisit>> GetEligibleVisitsForTransferAsync();
    }
}
