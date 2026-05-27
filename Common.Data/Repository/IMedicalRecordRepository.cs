using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity;

namespace Common.Data.Repository;

public interface IMedicalRecordRepository
{
    int Add(MedicalRecord record);
    Task<int> AddAsync(MedicalRecord record);
    void Delete(int id);
    Task DeleteAsync(int id);
    List<MedicalRecord> GetAll();
    Task<List<MedicalRecord>> GetAllAsync();
    List<MedicalRecord> GetByHistoryId(int historyId);
    Task<List<MedicalRecord>> GetByHistoryIdAsync(int historyId);
    MedicalRecord? GetById(int id);
    Task<MedicalRecord?> GetByIdAsync(int id);
    int? GetConsultingStaffId(int recordId);
    Task<int?> GetConsultingStaffIdAsync(int recordId);
    int GetERVisitCount(int patientId, DateTime fromDate);
    Task<int> GetERVisitCountAsync(int patientId, DateTime fromDate);
    Prescription? GetPrescription(int recordId);
    Task<Prescription?> GetPrescriptionAsync(int recordId);
    void Update(MedicalRecord record);
    Task UpdateAsync(MedicalRecord record);
}
