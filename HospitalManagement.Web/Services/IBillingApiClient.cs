namespace HospitalManagement.Web.Services;

public interface IBillingApiClient
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken);
    Task<decimal> ApplyDiscountAsync(decimal basePrice, int discountPercent, CancellationToken cancellationToken);
}
