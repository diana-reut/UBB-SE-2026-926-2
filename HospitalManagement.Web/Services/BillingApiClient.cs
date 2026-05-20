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

    public async Task<decimal> ComputeBasePriceAsync(
        int patientId,
        int recordId,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            $"{BaseUri}/base-price/{patientId}/{recordId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<decimal>(jsonOptions, cancellationToken);
    }

    public async Task<decimal> ApplyDiscountAsync(
        decimal basePrice,
        int discountPercent,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{BaseUri}/discount",
            new { BasePrice = basePrice, Discount = discountPercent },
            jsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<decimal>(jsonOptions, cancellationToken);
    }
}
