using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HospitalManagement.Web.Services;

public abstract class HospitalApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpContextAccessor httpContextAccessor;

    protected HospitalApiClientBase(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        HttpClient = httpClient;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, uri, null, cancellationToken);

    protected Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Post, uri, payload, cancellationToken);

    protected async Task PostAsync<TRequest>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Post, uri, payload, cancellationToken);
    }

    protected async Task PutAsync<TRequest>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Put, uri, payload, cancellationToken);
    }

    protected async Task DeleteAsync(string uri, CancellationToken cancellationToken = default)
    {
        _ = await SendAsync<object>(HttpMethod.Delete, uri, null, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string uri,
        object? payload,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddBearerToken(request);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: JsonOptions);
        }

        using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithMessageAsync(response, cancellationToken);

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
    }

    private void AddBearerToken(HttpRequestMessage request)
    {
        string? token = httpContextAccessor.HttpContext?.Session.GetString(WebSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Please sign in before using this page.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task EnsureSuccessWithMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorMessage = await ReadErrorMessageAsync(response, cancellationToken);
        string messageWithStatus = $"HTTP {(int)response.StatusCode} ({response.StatusCode}): {errorMessage}";
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(messageWithStatus);
        }

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
        {
            throw new ArgumentException(messageWithStatus);
        }

        throw new InvalidOperationException(messageWithStatus);
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            if (responseBody.TrimStart().StartsWith('"'))
            {
                return JsonSerializer.Deserialize<string>(responseBody) ?? "Request failed.";
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("detail", out JsonElement detail))
            {
                return detail.GetString() ?? "Request failed.";
            }

            if (root.TryGetProperty("errors", out JsonElement errors))
            {
                return errors.ToString();
            }

            if (root.TryGetProperty("title", out JsonElement title))
            {
                return title.GetString() ?? "Request failed.";
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }
}
