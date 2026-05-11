using System.Threading.Tasks;

namespace Common.API.Services;

public interface IBillingService
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId);

    Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount);
}
