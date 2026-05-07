using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IPrescriptionService
{
    public Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);

    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);

    public Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
