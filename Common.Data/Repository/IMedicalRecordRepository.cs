using HospitalManagement.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public interface IMedicalRecordRepository
{
    public Task<int> AddAsync(MedicalRecord record);

    public Task DeleteAsync(int id);

    public Task<List<MedicalRecord>> GetAll();

    public Task<List<MedicalRecord>> GetByHistoryIdAsync(int historyId);

    public Task<MedicalRecord?> GetByIdAsync(int id);

    public Task<int?> GetConsultingStaffIdAsync(int recordId);

    public Task<int> GetERVisitCountAsync(int patientId, DateTime fromDate);

    public Task<Prescription?> GetPrescriptionAsync(int recordId);

    public Task UpdateAsync(MedicalRecord record);

}

