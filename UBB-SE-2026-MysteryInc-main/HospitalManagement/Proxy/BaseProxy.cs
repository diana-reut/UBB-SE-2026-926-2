using System;
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
        HttpResponseMessage _ = response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Options);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
    {

        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(requestUri, data, Options);
        HttpResponseMessage _ = response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(Options);
    }

    // PUT: Update data
    protected async Task PutAsync<TRequest>(string uri, TRequest data)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(requestUri, data, Options);
        HttpResponseMessage _ = response.EnsureSuccessStatusCode();
    }

    // DELETE: Remove data
    protected async Task DeleteAsync(string uri)
    {
        var requestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        using HttpResponseMessage response = await HttpClient.DeleteAsync(requestUri);
        HttpResponseMessage _ = response.EnsureSuccessStatusCode();
    }
}