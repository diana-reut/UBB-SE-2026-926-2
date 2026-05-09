using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Integration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;

namespace Common.Data.Repository;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly EFHospitalDbContext _context;

    public PrescriptionRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);
        await _context.Prescriptions.AddAsync(prescription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        Prescription existing = await _context.Prescriptions
            .Include(p => p.MedicationList)
            .FirstOrDefaultAsync(p => p.Id == prescription.Id)
            ?? throw new KeyNotFoundException();

        _context.Entry(existing).CurrentValues.SetValues(prescription);
        _context.PrescriptionItems.RemoveRange(existing.MedicationList);
        existing.MedicationList.Clear();

        foreach (PrescriptionItem item in prescription.MedicationList)
        {
            existing.MedicationList.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Prescription? prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription is not null)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
        }
    }


    public Task<List<Prescription>> GetTopNAsync(int n, int page)
    {
        int pageSize = n <= 0 ? 20 : n;
        int pageNumber = page <= 0 ? 1 : page;

        return BaseQuery()
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId) =>
        _context.PrescriptionItems
            .Where(pi => pi.PrescriptionId == prescriptionId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return await GetTopNAsync(20, 1);
        }

        IQueryable<Prescription> query = BaseQuery()
            .Where(p => !p.MedicalRecord.History.Patient.IsArchived);

        if (filter.PrescriptionId.HasValue)
            query = query.Where(p => p.Id == filter.PrescriptionId.Value);

        if (filter.PatientId.HasValue)
            query = query.Where(p => p.MedicalRecord.History.PatientId == filter.PatientId.Value);

        if (filter.DoctorId.HasValue)
            query = query.Where(p => p.MedicalRecord.StaffId == filter.DoctorId.Value);

        if (!string.IsNullOrWhiteSpace(filter.MedName))
            query = query.Where(p => p.MedicationList.Any(m => m.MedName.Contains(filter.MedName)));

        if (filter.DateFrom.HasValue)
            query = query.Where(p => p.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(p => p.Date <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.PatientName))
        {
            query = query.Where(p =>
                p.MedicalRecord.History.Patient.FirstName.Contains(filter.PatientName) ||
                p.MedicalRecord.History.Patient.LastName.Contains(filter.PatientName));
        }

        return await query
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public Task<List<Prescription>> GetAllAsync() =>
        BaseQuery().ToListAsync();

    public Task<Prescription?> GetByRecordIdAsync(int recordId) =>
        BaseQuery().FirstOrDefaultAsync(p => p.RecordId == recordId);

    public List<Patient> GetAddictCandidatePatients() => GetAddictCandidatePatientsAsync().GetAwaiter().GetResult();

    public async Task<List<Patient>> GetAddictCandidatePatientsAsync()
    {
        DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
        DateTime now = DateTime.Now;

        List<int> candidatePatientIds = await _context.Prescriptions
            .Where(p => p.Date >= thirtyDaysAgo && p.Date <= now)
            .Where(p => !p.MedicalRecord.History.Patient.IsArchived)
            .SelectMany(
                p => p.MedicationList,
                (prescription, medication) => new
                {
                    PatientId = prescription.MedicalRecord.History.PatientId,
                    DoctorId = prescription.MedicalRecord.StaffId,
                    MedName = medication.MedName,
                })
            .GroupBy(x => new { x.PatientId, x.MedName })
            .Where(g => g.Select(x => x.DoctorId).Distinct().Count() >= 3) // Patients with the same medication prescribed by 3 or more different doctors
            .Select(g => g.Key.PatientId)
            .Distinct()
            .ToListAsync();

        return await _context.Patients
            .Where(p => candidatePatientIds.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    private IQueryable<Prescription> BaseQuery() =>
        _context.Prescriptions
            .Include(p => p.MedicationList)
            .Include(p => p.MedicalRecord)
            .ThenInclude(mr => mr.History)
            .ThenInclude(mh => mh.Patient)
            .AsNoTracking();
}
