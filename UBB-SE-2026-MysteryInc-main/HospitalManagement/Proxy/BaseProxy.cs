using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy;

public abstract class ProxyBase
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions Options;

    protected ProxyBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
        Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    protected async Task<T?> GetAsync<T>(string uri)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(uri);
        await EnsureSuccessWithMessageAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Options);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(requestUri, data, Options);
        await EnsureSuccessWithMessageAsync(response);

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseBody, Options);
    }

    protected async Task PutAsync<TRequest>(string uri, TRequest data)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(requestUri, data, Options);
        await EnsureSuccessWithMessageAsync(response);
    }

    protected async Task DeleteAsync(string uri)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.DeleteAsync(requestUri);
        await EnsureSuccessWithMessageAsync(response);
    }

    private static async Task EnsureSuccessWithMessageAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? errorMessage = null;
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                if (responseBody.TrimStart().StartsWith('"'))
                {
                    errorMessage = JsonSerializer.Deserialize<string>(responseBody);
                }
                else
                {
                    using JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                    JsonElement root = jsonDocument.RootElement;

                    if (root.TryGetProperty("detail", out JsonElement detailElement))
                    {
                        errorMessage = detailElement.GetString();
                    }
                    else if (root.TryGetProperty("title", out JsonElement titleElement))
                    {
                        errorMessage = titleElement.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                errorMessage = responseBody;
            }
        }

        errorMessage ??= $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";

        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ArgumentException(errorMessage);
        }

        throw new InvalidOperationException(errorMessage);
    }
}
