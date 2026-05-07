using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

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
            => AddAsync(log).GetAwaiter().GetResult();

        public async Task AddAsync(Transfer_Log log)
        {
            await context.TransferLogs.AddAsync(log);
            await context.SaveChangesAsync();
            Logger.Info($"[TransferLogRepository] Added log entry {log.Transfer_ID} for Visit {log.Visit_ID}, Status={log.Status}");
        }

        public List<Transfer_Log> GetByVisitId(int id)
            => GetByVisitIdAsync(id).GetAwaiter().GetResult();

        public Task<List<Transfer_Log>> GetByVisitIdAsync(int id)
        {
            return context.TransferLogs
                .AsNoTracking()
                .Where(log => log.Visit_ID == id)
                .OrderByDescending(log => log.Transfer_Time)
                .ToListAsync();
        }

        public List<Transfer_Log> GetAll()
            => GetAllAsync().GetAwaiter().GetResult();

        public Task<List<Transfer_Log>> GetAllAsync()
        {
            return context.TransferLogs
                .AsNoTracking()
                .OrderByDescending(log => log.Transfer_Time)
                .ToListAsync();
        }

        public void DeleteLog(Transfer_Log log)
            => DeleteLogAsync(log).GetAwaiter().GetResult();

        public async Task DeleteLogAsync(Transfer_Log log)
        {
            context.TransferLogs.Remove(log);
            await context.SaveChangesAsync();
        }

        public void UpdateStatus(int transferId, string newStatus)
            => UpdateStatusAsync(transferId, newStatus).GetAwaiter().GetResult();

        public async Task UpdateStatusAsync(int transferId, string newStatus)
        {
            Transfer_Log log = await context.TransferLogs.FirstAsync(l => l.Transfer_ID == transferId);
            log.Status = newStatus;
            await context.SaveChangesAsync();
        }
    }
}
