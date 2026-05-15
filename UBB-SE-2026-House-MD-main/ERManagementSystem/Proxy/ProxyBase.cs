using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ERManagementSystem.Proxy;

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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Options);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
    {
        Uri requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(requestUri, data, Options);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(Options);
    }

    protected async Task PostAsync(string uri)
    {
        Uri requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PostAsync(requestUri, content: null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task PutAsync<TRequest>(string uri, TRequest data)
    {
        Uri requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(requestUri, data, Options);
        response.EnsureSuccessStatusCode();
    }

    protected async Task DeleteAsync(string uri)
    {
        Uri requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();
    }

    protected Task DeleteRequestAsync(string uri)
    {
        return DeleteAsync(uri);
    }
}
