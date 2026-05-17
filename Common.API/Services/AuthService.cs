using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Repository;
using Microsoft.IdentityModel.Tokens;

namespace Common.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        User? user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user is null || !VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        string token = GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Role);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        bool exists = await _userRepository.ExistsByUsernameAsync(dto.Username);
        if (exists)
            throw new ArgumentException($"Username '{dto.Username}' is already taken.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            Role = dto.Role
        };

        await _userRepository.CreateAsync(user);

        string token = GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Role);
    }

    private string GenerateToken(User user)
    {
        string secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
        string issuer = _config["Jwt:Issuer"] ?? "HospitalAPI";
        string audience = _config["Jwt:Audience"] ?? "HospitalClients";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
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

    private static bool VerifyPassword(string password, string storedHash)
    {
        byte[] combined = Convert.FromBase64String(storedHash);
        byte[] salt = combined[..16];
        byte[] expectedHash = combined[16..];

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
