using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Proxy.AuthProxy;

internal class AuthProxy : ProxyBase, IAuthProxy
{
    private const string BaseUri = "api/auth";

    public AuthProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        return await PostAsync<LoginDto, AuthResponseDto>($"{BaseUri}/login", dto)
            ?? throw new UnauthorizedAccessException("Login returned no response.");
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        return await PostAsync<RegisterDto, AuthResponseDto>($"{BaseUri}/register", dto)
            ?? throw new UnauthorizedAccessException("Register returned no response.");
    }
}
