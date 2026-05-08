using System.Threading.Tasks;

namespace HospitalManagement.Proxy.BillingProxy;

public interface IBillingProxy
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId);

    Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount);
}