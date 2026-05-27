using Common.Data.Data;
using Common.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class UserRepository : IUserRepository
{
    private readonly EFHospitalDbContext db;

    public UserRepository(EFHospitalDbContext db)
    {
        this.db = db;
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        return db.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
