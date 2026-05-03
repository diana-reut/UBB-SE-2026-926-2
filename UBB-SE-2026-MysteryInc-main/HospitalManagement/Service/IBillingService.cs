namespace HospitalManagement.Service;

internal interface IBillingService
{
    public decimal ApplyDiscount(decimal basePrice, int discount);

    public decimal ComputeBasePrice(int patientId, int recordId);
}
