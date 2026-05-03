using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IAddictDetectionService
{
    public string BuildPoliceReport(Patient patient);

    public List<Patient> GetAddictCandidates();
}
