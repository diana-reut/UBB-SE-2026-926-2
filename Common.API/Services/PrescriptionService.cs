using Common.Data.Entity;
using Common.Data.Repository;
using Common.Data.Integration;
using Common.Data;

namespace Common.API.Service;

internal class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository prescriptionRepository;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository)
    {
        this.prescriptionRepository = prescriptionRepository ?? throw new ArgumentNullException(nameof(prescriptionRepository));
    }

    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page)
    {
        return prescriptionRepository.GetTopNAsync(n, page);
    }

    public async Task<Prescription> GetPrescriptionDetailsAsync(int id)
    {
        var filter = new PrescriptionFilter { PrescriptionId = id, };
        List<Prescription> prescriptions = await prescriptionRepository.GetFilteredAsync(filter);
        return prescriptions.FirstOrDefault() ?? throw new ArgumentException($"Prescription with ID {id} does not exist.");
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return await prescriptionRepository.GetTopNAsync(20, 1);
        }

        try
        {
            return await prescriptionRepository.GetFilteredAsync(filter);
        }
        catch (Exception)
        {
            throw new MyNotImplementedException("The medication search could not be completed at this time due to high system load or complex parameters. Please try simplifying your search or try again later.");
        }
    }
}
