using Common.Data.Entity;
using Common.Data.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.API.Service;

public interface IPrescriptionService
{
    public Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);

    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);

    public Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
