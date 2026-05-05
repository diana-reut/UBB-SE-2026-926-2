using Common.Data.Entity;
using HospitalManagement.Data;
using HospitalManagement.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal class MedicalHistoryRepository : IMedicalHistoryRepository
{
    private readonly EFHospitalDbContext _context;

    public MedicalHistoryRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<MedicalHistory?> GetByPatientIdAsync(int patientId)
    {
        return _context.MedicalHistory
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(h => h.Patient.Id == patientId);
    }

    public Task<MedicalHistory?> GetByIdAsync(int historyId)
    {
        return _context.MedicalHistory
            .FirstOrDefaultAsync(h => h.Id == historyId);
    }

    public async Task<int> CreateAsync(MedicalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        _ = _context.MedicalHistory.Add(history);
        _ = await _context.SaveChangesAsync();

        return history.Id;
    }

    public async Task UpdateAsync(MedicalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        var existing = await _context.MedicalHistory
            .AsNoTracking()
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(h => h.Id == history.Id)
            ?? throw new KeyNotFoundException($"MedicalHistory with ID={history.Id} not found.");

        if (existing.Patient.Id != history.Patient.Id)
        {
            throw new InvalidOperationException("PatientID mismatch - cannot reassign history to a different patient.");
        }

        _ = _context.MedicalHistory.Update(history);
        _ = await _context.SaveChangesAsync();
    }

    public async Task SaveAllergiesAsync(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies)
    {
        if (allergies is null || allergies.Count == 0)
            return;

        foreach (var (allergy, severity) in allergies)
        {
            var patientAllergy = new PatientAllergy
            {
                MedicalHistoryId = historyId,
                Allergy = allergy,
                SeverityLevel = severity,
            };
            _context.PatientAllergies.Add(patientAllergy);
        }

        _ = await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetChronicConditionsAsync(int historyId)
    {
        var history = await _context.MedicalHistory
            .Where(h => h.Id == historyId)
            .Select(h => h.ChronicConditions)
            .FirstOrDefaultAsync();

        return history ?? [];
    }

    public async Task<List<(Allergy Allergy, string SeverityLevel)>> GetAllergiesByHistoryIdAsync(int historyId)
    {
        var patientAllergies = await _context.PatientAllergies
            .Include(pa => pa.Allergy)
            .Where(pa => pa.MedicalHistoryId == historyId)
            .ToListAsync();

        return patientAllergies
            .Select(pa => (pa., pa.SeverityLevel))
            .ToList();
    }

    // INCLUDE HISTORY IN THE ALLERGY 
}