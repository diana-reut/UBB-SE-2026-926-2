using System.Collections.Generic;
using ERManagementSystem.Models;

namespace ERManagementSystem.Services
{
    public interface ITransferService
    {
        Transfer_Log SendPatientData(int visitId);
        void LogTransfer(int visitId, string status);
        List<Transfer_Log> GetLogs(int visitId);
        Transfer_Log RetryTransfer(int visitId);
        void MarkPatientAsTransferred(int visitId);
        void TransitionVisitToTransferred(int visitId);
        void CloseVisit(int visitId);
        List<TransferEligibleVisit> GetEligibleVisitsForTransfer();
    }
}
