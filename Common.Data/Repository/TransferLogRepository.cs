using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class TransferLogRepository : ITransferLogRepository
{
    private readonly EFHospitalDbContext context;

    public TransferLogRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public Task<List<Transfer_Log>> GetAllAsync() =>
        context.TransferLogs.AsNoTracking().ToListAsync();

    public Task<Transfer_Log?> GetByIdAsync(int id) =>
        context.TransferLogs.AsNoTracking().FirstOrDefaultAsync(t => t.Transfer_ID == id);

    public async Task<Transfer_Log> CreateAsync(Transfer_Log transferLog)
    {
        await context.TransferLogs.AddAsync(transferLog);
        await context.SaveChangesAsync();
        return transferLog;
    }

    public async Task<bool> UpdateAsync(int id, Transfer_Log transferLog)
    {
        Transfer_Log? existingTransferLog = await context.TransferLogs.FirstOrDefaultAsync(t => t.Transfer_ID == id);
        if (existingTransferLog is null)
        {
            return false;
        }

        existingTransferLog.Visit_ID = transferLog.Visit_ID;
        existingTransferLog.Transfer_Time = transferLog.Transfer_Time;
        existingTransferLog.Target_System = transferLog.Target_System;
        existingTransferLog.FilePath = transferLog.FilePath;
        existingTransferLog.Status = transferLog.Status;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Transfer_Log? transferLog = await context.TransferLogs.FirstOrDefaultAsync(t => t.Transfer_ID == id);
        if (transferLog is null)
        {
            return false;
        }

        context.TransferLogs.Remove(transferLog);
        await context.SaveChangesAsync();
        return true;
    }
}
