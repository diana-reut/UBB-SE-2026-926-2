using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IPrescriptionService
{
    public List<Prescription> ApplyFilter(PrescriptionFilter filter);
    public Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);

    public List<Prescription> GetLatestPrescriptions(int n, int page);
    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);

    public Prescription GetPrescriptionDetails(int id);
    public Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
