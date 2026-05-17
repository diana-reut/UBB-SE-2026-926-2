using System.Threading.Tasks;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Proxy.AuthProxy;

public interface IAuthProxy
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
}
