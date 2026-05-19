using System.Security.Cryptography;
using System.Text;
using Common.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.Data;

public static class DatabaseSeeder
{
    private static readonly (string Username, string Password, string Role)[] SeedAccounts =
    [
        ("admin", "Admin123!", "Admin"),
        ("medic", "Medic123!", "Medic"),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        EFHospitalDbContext db = scope.ServiceProvider.GetRequiredService<EFHospitalDbContext>();

        await db.Database.MigrateAsync();

        foreach ((string username, string password, string role) in SeedAccounts)
        {
            bool exists = await db.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                continue;
            }

            db.Users.Add(new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role,
            });
        }

        await db.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        byte[] combined = new byte[salt.Length + hash.Length];
        salt.CopyTo(combined, 0);
        hash.CopyTo(combined, salt.Length);
        return Convert.ToBase64String(combined);
    }
}
