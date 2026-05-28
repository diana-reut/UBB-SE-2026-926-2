using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class AuthenticationApiClient : IAuthenticationApiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AuthenticationApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AuthResponseDto> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var dto = new LoginDto
        {
            Username = username,
            Password = password
        };

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            dto,
            jsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(
                jsonOptions,
                cancellationToken);

            return authResponse ?? throw new InvalidOperationException("Login returned an empty response.");
        }

        string errorMessage = await ApiErrorReader.ReadErrorMessageAsync(response, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(errorMessage);
        }

        throw new InvalidOperationException(errorMessage);
    }
}
