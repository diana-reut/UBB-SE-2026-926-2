using Common.Data.Entity.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace HospitalManagement.Web.Services;

public class BillingApiClient : IBillingApiClient
{
    private const string BaseUri = "api/billing";

    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public BillingApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                $"{BaseUri}/base-price/{patientId}/{recordId}", cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<decimal>(jsonOptions, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the billing API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The billing API request timed out or was interrupted.");
        }
    }

    public async Task<decimal> ApplyDiscountAsync(int recordId, decimal basePrice, int discount, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{BaseUri}/discount/{recordId}",
                new ApplyDiscountRequestDto { BasePrice = basePrice, Discount = discount },
                jsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<decimal>(jsonOptions, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the billing API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The billing API request timed out or was interrupted.");
        }
    }
}