using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace ERManagementSystem.Proxy.PatientProxy;

public interface IPatientProxy
{
    Task<Patient?> GetByCnpAsync(string cnp);
    Task<bool> ExistsAsync(string cnp);
    Task<Patient> CreatePatientAsync(CreatePatientDto dto);
    Task UpdatePatientAsync(Patient patient);
}
