using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class ERRoomService : IERRoomService
{
    private readonly IERRoomRepository _repository;

    public ERRoomService(IERRoomRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ER_Room>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<ER_Room?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<ER_Room> CreateAsync(ER_Room room) =>
        _repository.CreateAsync(room);

    public Task<bool> UpdateAsync(int id, ER_Room room) =>
        _repository.UpdateAsync(id, room);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}
