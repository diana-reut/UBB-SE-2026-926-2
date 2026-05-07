using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public interface IPrescriptionRepository
{
    void Add(Prescription prescription);
    Task AddAsync(Prescription prescription);
    void Delete(int id);
    Task DeleteAsync(int id);
    List<Patient> GetAddictCandidatePatients();
    Task<List<Patient>> GetAddictCandidatePatientsAsync();
    List<Prescription> GetAll();
    Task<List<Prescription>> GetAllAsync();
    Prescription? GetByRecordId(int recordId);
    Task<Prescription?> GetByRecordIdAsync(int recordId);
    List<Prescription> GetFiltered(PrescriptionFilter filter);
    Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter);
    List<PrescriptionItem> GetItems(int prescriptionId);
    Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId);
    List<Prescription> GetTopN(int n, int page);
    Task<List<Prescription>> GetTopNAsync(int n, int page);
    void Update(Prescription prescription);
    Task UpdateAsync(Prescription prescription);
}
