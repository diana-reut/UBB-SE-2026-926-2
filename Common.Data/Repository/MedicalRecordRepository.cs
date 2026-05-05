using HospitalManagement.Data;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly EFHospitalDbContext _context;

    public MedicalRecordRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<MedicalRecord>> GetByHistoryIdAsync(int historyId)
    {
        return _context.MedicalRecords
            .Include(r => r.History)
            .Where(r => r.History.Id == historyId)
            .ToListAsync();
    }

    public Task<MedicalRecord?> GetByIdAsync(int id)
    {
        return _context.MedicalRecords
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<int> AddAsync(MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        _ =_context.MedicalRecords.Add(record);
        _ = await _context.SaveChangesAsync();

        return record.Id;
    }

    public async Task UpdateAsync(MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        _ = _context.MedicalRecords.Update(record);
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var record = await _context.MedicalRecords
            .Include(r => r.Prescription)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record is not null)
        {
            _ = _context.MedicalRecords.Remove(record);
            _ = await _context.SaveChangesAsync();
        }
    }

    public Task<int> GetERVisitCountAsync(int patientId, DateTime fromDate)
    {
        return _context.MedicalRecords
            .Include(r => r.History)
                .ThenInclude(rh => rh.Patient)
            .CountAsync(r => r.History.Patient.Id == patientId
                && r.SourceType == SourceType.ER
                && r.ConsultationDate >= fromDate);
    }

    public Task<Prescription?> GetPrescriptionAsync(int recordId)
    {

        return _context.Prescriptions
            .FirstOrDefaultAsync(p => p.RecordId == recordId);
    }

    public Task<int?> GetConsultingStaffIdAsync(int recordId)
    {
        return _context.MedicalRecords
            .Where(r => r.Id == recordId)
            .Select(r => (int?)r.StaffId)
            .FirstOrDefaultAsync();
    }

    public Task<List<MedicalRecord>> GetAll()
    {
        return _context.MedicalRecords.AsNoTracking().ToListAsync();
    }
}
