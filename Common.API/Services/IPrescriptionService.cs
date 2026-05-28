using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Integration;

namespace Common.API.Service;

public interface IPrescriptionService
{
    public Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);

    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);

    public Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
