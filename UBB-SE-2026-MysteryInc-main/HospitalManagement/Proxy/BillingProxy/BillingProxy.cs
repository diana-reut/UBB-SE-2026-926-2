using Common.Data.Entity.DTOs;
using System.Net.Http;
using System.Threading.Tasks;
using HospitalManagement.Proxy;

namespace HospitalManagement.Proxy.BillingProxy;

internal class BillingProxy : ProxyBase, IBillingProxy
{
    private const string BaseUri = "api/billing";

    public BillingProxy(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
    {
        return await GetAsync<decimal>($"{BaseUri}/base-price/{patientId}/{recordId}");
    }

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
    {
        ApplyDiscountRequestDto dto = new()
        {
            BasePrice = basePrice,
            Discount = discount
        };

        return await PostAsync<ApplyDiscountRequestDto, decimal>($"{BaseUri}/discount", dto);
    }
}