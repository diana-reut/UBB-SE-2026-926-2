using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public interface IAuthenticationApiClient
{
    Task<AuthResponseDto> LoginAsync(string username, string password, CancellationToken cancellationToken);
}
