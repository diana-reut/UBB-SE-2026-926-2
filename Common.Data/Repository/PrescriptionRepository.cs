using HospitalManagement.Data;
using HospitalManagement.Entity;
using HospitalManagement.Entity.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal class PrescriptionRepository : IPrescriptionRepository
{
    private readonly EFHospitalDbContext _context;

    public PrescriptionRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<Prescription?> GetByRecordIdAsync(int recordId)
    {
        return _context.Prescriptions
            .Include(p => p.MedicationList)
            .FirstOrDefaultAsync(p => p.RecordId == recordId);
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

        var existing = await _context.Prescriptions
            .Include(p => p.MedicationList)
            .FirstOrDefaultAsync(p => p.Id == prescription.Id);

        if (existing == null) throw new KeyNotFoundException();

        _context.Entry(existing).CurrentValues.SetValues(prescription);

        // Replace medication list
        _context.PrescriptionItems.RemoveRange(existing.MedicationList);
        foreach (var item in prescription.MedicationList)
        {
            existing.MedicationList.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription != null)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
        }
    }

    public Task<List<Prescription>> GetTopNAsync(int n, int page)
    {
        int pageSize = n <= 0 ? 20 : n;
        int pageNumber = page <= 0 ? 1 : page;

        return _context.Prescriptions
            .Include(p => p.MedicationList)
            .Include(p => p.MedicalRecord)
                .ThenInclude(mr => mr.History)
                .ThenInclude(mh => mh.Patient)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId)
    {
        return _context.PrescriptionItems
            .Where(pi => pi.PrescriptionId == prescriptionId)
            .ToListAsync();
    }

    public async Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter)
    {
        if (filter is null) return await GetTopNAsync(20, 1);

        var query = _context.Prescriptions
            .Include(p => p.MedicationList)
            .Include(p => p.MedicalRecord)
                .ThenInclude(mr => mr.History)
                .ThenInclude(mh => mh.Patient)
            .Where(p => p.MedicalRecord.History.Patient.IsArchived == false)
            .AsQueryable();

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

        return await query.OrderByDescending(p => p.Date).ToListAsync();
    }

    public Task<List<Prescription>> GetAllAsync()
    {
        return _context.Prescriptions
            .Include(p => p.MedicationList)
            .ToListAsync();
    }

    public async Task<List<Patient>> GetAddictCandidatePatientsAsync()
    {
        DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

        return await _context.Prescriptions
            .Where(p => p.Date >= thirtyDaysAgo)
            .Where(p => !p.MedicalRecord.History.Patient.IsArchived)
            .GroupBy(p => new { p.MedicalRecord.History.Patient, p.MedicationList.FirstOrDefault().MedName })
            .Where(g => g.Select(p => p.MedicalRecord.StaffId).Distinct().Count() >= 3)
            .Select(g => g.Key.Patient)
            .Distinct()
            .ToListAsync();
    }
}