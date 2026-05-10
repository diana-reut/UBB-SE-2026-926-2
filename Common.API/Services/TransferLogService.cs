using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class TransferLogService : ITransferLogService
{
    private readonly ITransferLogRepository _repository;

    public TransferLogService(ITransferLogRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Transfer_Log>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Transfer_Log?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<Transfer_Log> CreateAsync(Transfer_Log transferLog) =>
        _repository.CreateAsync(transferLog);

    public Task<bool> UpdateAsync(int id, Transfer_Log transferLog) =>
        _repository.UpdateAsync(id, transferLog);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
