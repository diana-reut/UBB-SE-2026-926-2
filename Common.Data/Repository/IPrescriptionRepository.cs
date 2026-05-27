using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity;
using Common.Data.Integration;
using Common.Data.Integration;

namespace Common.Data.Repository;

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
    Task MarkPoliceNotifiedAsync(int patientId);
    Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds);
}
