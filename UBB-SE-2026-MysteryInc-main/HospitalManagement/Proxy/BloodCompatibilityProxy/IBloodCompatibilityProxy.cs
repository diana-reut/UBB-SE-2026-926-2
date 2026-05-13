using Common.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HospitalManagement.Proxy.BloodCompatibilityProxy;

internal interface IBloodCompatibilityProxy
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
}