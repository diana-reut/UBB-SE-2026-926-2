using Common.Data.Entity;
using Common.Data.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.PrescriptionProxy;

internal interface IPrescriptionProxy
{
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);

    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);

    Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
