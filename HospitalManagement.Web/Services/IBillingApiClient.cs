namespace HospitalManagement.Web.Services;

public interface IBillingApiClient
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken);
    Task<decimal> ApplyDiscountAsync(int recordId, decimal basePrice, int discount, CancellationToken cancellationToken);
}