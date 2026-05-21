using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public class BillingApiClient : HospitalApiClientBase, IBillingApiClient
{
    private const string BaseUri = "api/billing";

    public BillingApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<decimal> ComputeBasePriceAsync(
        int patientId,
        int recordId,
        CancellationToken cancellationToken = default) =>
        await GetAsync<decimal>($"{BaseUri}/base-price/{patientId}/{recordId}", cancellationToken);

    public async Task<decimal> ApplyDiscountAsync(
        decimal basePrice,
        int discount,
        CancellationToken cancellationToken = default)
    {
        var dto = new ApplyDiscountRequestDto
        {
            BasePrice = basePrice,
            Discount = discount
        };

        return await PostAsync<ApplyDiscountRequestDto, decimal>($"{BaseUri}/discount", dto, cancellationToken);
    }
}