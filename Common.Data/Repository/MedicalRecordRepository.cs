using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly EFHospitalDbContext context;

    public MedicalRecordRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public int Add(MedicalRecord record) => AddAsync(record).GetAwaiter().GetResult();

    public async Task<int> AddAsync(MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _ = context.MedicalRecords.Add(record);
        _ = await context.SaveChangesAsync();
        return record.Id;
    }

    public void Update(MedicalRecord record) => UpdateAsync(record).GetAwaiter().GetResult();

    public async Task UpdateAsync(MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _ = context.MedicalRecords.Update(record);
        _ = await context.SaveChangesAsync();
    }

    public void Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();

    public async Task DeleteAsync(int id)
    {
        MedicalRecord? record = await context.MedicalRecords
            .Include(r => r.Prescription)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record is not null)
        {
            _ = context.MedicalRecords.Remove(record);
            _ = await context.SaveChangesAsync();
        }
    }

    public List<MedicalRecord> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    public Task<List<MedicalRecord>> GetAllAsync() =>
        context.MedicalRecords
            .Include(r => r.History)
            .AsNoTracking()
            .ToListAsync();

    public List<MedicalRecord> GetByHistoryId(int historyId) => GetByHistoryIdAsync(historyId).GetAwaiter().GetResult();

    public Task<List<MedicalRecord>> GetByHistoryIdAsync(int historyId) =>
        context.MedicalRecords
            .Include(r => r.History)
            .Where(r => r.HistoryId == historyId)
            .AsNoTracking()
            .ToListAsync();

    public MedicalRecord? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();

    public Task<MedicalRecord?> GetByIdAsync(int id) =>
        context.MedicalRecords
            .Include(r => r.History)
            .Include(r => r.Prescription)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

    public int? GetConsultingStaffId(int recordId) => GetConsultingStaffIdAsync(recordId).GetAwaiter().GetResult();

    public Task<int?> GetConsultingStaffIdAsync(int recordId) =>
        context.MedicalRecords
            .Where(r => r.Id == recordId)
            .Select(r => (int?)r.StaffId)
            .FirstOrDefaultAsync();

    public int GetERVisitCount(int patientId, DateTime fromDate) => GetERVisitCountAsync(patientId, fromDate).GetAwaiter().GetResult();

    public Task<int> GetERVisitCountAsync(int patientId, DateTime fromDate) =>
        context.MedicalRecords
            .Include(r => r.History)
            .CountAsync(r => r.History.PatientId == patientId
                && r.SourceType == SourceType.ER
                && r.ConsultationDate >= fromDate);

    public Prescription? GetPrescription(int recordId) => GetPrescriptionAsync(recordId).GetAwaiter().GetResult();

    public Task<Prescription?> GetPrescriptionAsync(int recordId) =>
        context.Prescriptions
            .Include(p => p.MedicationList)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RecordId == recordId);
}
