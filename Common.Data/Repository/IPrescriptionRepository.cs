using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public interface IPrescriptionRepository
{
    Task AddAsync(Prescription prescription);
    Task DeleteAsync(int id);
    Task<List<Patient>> GetAddictCandidatePatientsAsync();
    Task<List<Prescription>> GetAllAsync();
    Task<Prescription?> GetByRecordIdAsync(int recordId);
    Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter);
    Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId);
    Task<List<Prescription>> GetTopNAsync(int n, int page);
    Task UpdateAsync(Prescription prescription);
}
