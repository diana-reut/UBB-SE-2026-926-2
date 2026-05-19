using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class AuthenticationApiClient : IAuthenticationApiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new()
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
        var dto = new LoginDto(username, password);

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

        string errorMessage = await ReadErrorMessageAsync(response, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(errorMessage);
        }

        throw new InvalidOperationException(errorMessage);
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"Login failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            if (responseBody.TrimStart().StartsWith('"'))
            {
                return JsonSerializer.Deserialize<string>(responseBody) ?? "Login failed.";
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("detail", out JsonElement detail))
            {
                return detail.GetString() ?? "Login failed.";
            }

            if (root.TryGetProperty("title", out JsonElement title))
            {
                return title.GetString() ?? "Login failed.";
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }
}
