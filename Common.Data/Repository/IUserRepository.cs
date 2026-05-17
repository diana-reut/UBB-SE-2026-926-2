using Common.Data.Entity;

namespace Common.Data.Repository;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
}
