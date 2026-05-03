using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IPrescriptionService
{
    public List<Prescription> ApplyFilter(PrescriptionFilter filter);

    public List<Prescription> GetLatestPrescriptions(int n, int page);

    public Prescription GetPrescriptionDetails(int id);
}
