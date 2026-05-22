using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class BillingApiClient : HospitalApiClientBase, IBillingApiClient
{
    private const string BaseUri = "api/billing";

    public BillingApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken)
    {
        try
        {
            return await GetAsync<decimal>(
                $"{BaseUri}/base-price/{patientId}/{recordId}", cancellationToken);
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
            return await PostAsync<ApplyDiscountRequestDto, decimal>(
                $"{BaseUri}/discount/{recordId}",
                new ApplyDiscountRequestDto { BasePrice = basePrice, Discount = discount },
                cancellationToken);
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