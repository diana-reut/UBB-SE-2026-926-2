using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class ERRoomRepository : IERRoomRepository
{
    private readonly EFHospitalDbContext context;

    public ERRoomRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public Task<List<ER_Room>> GetAllAsync() =>
        context.ERRooms.AsNoTracking().ToListAsync();

    public Task<ER_Room?> GetByIdAsync(int id) =>
        context.ERRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Room_ID == id);

    public async Task<ER_Room> CreateAsync(ER_Room room)
    {
        await context.ERRooms.AddAsync(room);
        await context.SaveChangesAsync();
        return room;
    }

    public async Task<bool> UpdateAsync(int id, ER_Room room)
    {
        ER_Room? existingRoom = await context.ERRooms.FirstOrDefaultAsync(r => r.Room_ID == id);
        if (existingRoom is null)
        {
            return false;
        }

        existingRoom.Room_Type = room.Room_Type;
        existingRoom.Availability_Status = room.Availability_Status;
        existingRoom.Current_Visit_ID = room.Current_Visit_ID;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ER_Room? room = await context.ERRooms.FirstOrDefaultAsync(r => r.Room_ID == id);
        if (room is null)
        {
            return false;
        }

        context.ERRooms.Remove(room);
        await context.SaveChangesAsync();
        return true;
    }
}
