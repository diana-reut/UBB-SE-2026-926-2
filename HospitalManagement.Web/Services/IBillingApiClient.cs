namespace HospitalManagement.Web.Services;

public interface IBillingApiClient
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken = default);
    Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount, CancellationToken cancellationToken = default);
}