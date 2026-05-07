using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Helpers;
using Common.Data.Data;
using Microsoft.EntityFrameworkCore;
using Common.Data.Entity;
using Common.Data.Models;

namespace ERManagementSystem.Repositories
{
    public class TransferLogRepository : ITransferLogRepository
    {
        private readonly EFHospitalDbContext context;

        public TransferLogRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(Transfer_Log log)
        {
            context.TransferLogs.Add(log);
            context.SaveChanges();
            Logger.Info($"[TransferLogRepository] Added log entry {log.Transfer_ID} for Visit {log.Visit_ID}, Status={log.Status}");
        }

        public List<Transfer_Log> GetByVisitId(int id)
        {
            return context.TransferLogs
                .AsNoTracking()
                .Where(log => log.Visit_ID == id)
                .OrderByDescending(log => log.Transfer_Time)
                .ToList();
        }

        public List<Transfer_Log> GetAll()
        {
            return context.TransferLogs
                .AsNoTracking()
                .OrderByDescending(log => log.Transfer_Time)
                .ToList();
        }

        public void DeleteLog(Transfer_Log log)
        {
            context.TransferLogs.Remove(log);
            context.SaveChanges();
        }

        public void UpdateStatus(int transferId, string newStatus)
        {
            Transfer_Log log = context.TransferLogs.First(l => l.Transfer_ID == transferId);
            log.Status = newStatus;
            context.SaveChanges();
        }
    }
}
