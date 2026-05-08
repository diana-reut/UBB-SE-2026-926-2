using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IBillingService
{
    public decimal ApplyDiscount(decimal basePrice, int discount);
    public Task<decimal> ComputeBasePriceAsync(int patientId, int recordId);
}
