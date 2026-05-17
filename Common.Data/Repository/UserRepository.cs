using Common.Data.Data;
using Common.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class UserRepository : IUserRepository
{
    private readonly EFHospitalDbContext _db;

    public UserRepository(EFHospitalDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        return _db.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
