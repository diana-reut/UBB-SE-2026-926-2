using HospitalManagement.Data;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal class PatientRepository : IPatientRepository
{
    private readonly EFHospitalDbContext _context;

    public PatientRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<Patient?> GetByIdAsync(int id)
    {
        return _context.Patients
            .Include(p => p.MedicalHistory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<Patient>> GetAllAsync(bool includeArchived)
    {
        IQueryable<Patient> query = _context.Patients;

        if (!includeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return query.ToListAsync();
    }

    public Task<List<Patient>> GetArchivedAsync()
    {
        return _context.Patients
            .Where(p => p.IsArchived)
            .ToListAsync();
    }

    public Task<List<Patient>> SearchAsync(PatientFilter patientFilter)
    {
        ArgumentNullException.ThrowIfNull(patientFilter);

        // Include MedicalHistory to ensure filtering on BloodType and ChronicConditions works
        var query = _context.Patients
            .Include(p => p.MedicalHistory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientFilter.NamePart))
        {
            query = query.Where(p => p.FirstName.Contains(patientFilter.NamePart)
                || p.LastName.Contains(patientFilter.NamePart));
        }

        if (!string.IsNullOrWhiteSpace(patientFilter.CNP))
        {
            query = query.Where(p => p.Cnp.StartsWith(patientFilter.CNP));
        }

        int currentYear = DateTime.Now.Year;
        if (patientFilter.MinAge.HasValue)
        {
            query = query.Where(p => currentYear - p.Dob.Year >= patientFilter.MinAge);
        }

        if (patientFilter.MaxAge.HasValue)
        {
            query = query.Where(p => currentYear - p.Dob.Year <= patientFilter.MaxAge);
        }

        if (patientFilter.BloodType.HasValue)
        {
            query = query.Where(p => p.MedicalHistory != null && p.MedicalHistory.BloodType == patientFilter.BloodType);
        }

        if (patientFilter.Sex.HasValue)
        {
            query = query.Where(p => p.Sex == patientFilter.Sex);
        }

        if (patientFilter.HasChronicCond == true)
        {
            query = query.Where(p => p.MedicalHistory != null && p.MedicalHistory.ChronicConditions.Any());
        }

        return query.ToListAsync();
    }

    public async Task AddAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);

        _ = await _context.Patients.AddAsync(p);
        _ = await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);

        _ = _context.Patients.Update(p);
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient is not null)
        {
            _ = _context.Patients.Remove(patient);
            _ = await _context.SaveChangesAsync();
        }
    }

    public Task<bool> ExistsAsync(string cnp)
    {
        return _context.Patients.AnyAsync(p => p.Cnp == cnp);
    }

    public async Task MarkAsDeceasedAsync(int id, DateTime dod)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient is not null)
        {
            patient.Dod = dod;
            _ = await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Patient>> GetCompatibleDonorsAsync(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge)
    {
        int currentYear = DateTime.Now.Year;
        int recipientAge = currentYear - dob.Year;

        var donors = await _context.Patients
            .Include(p => p.MedicalHistory)
            .ThenInclude(mh => mh.Allergies)
            .Where(p => !p.IsArchived && p.MedicalHistory != null)
            // Age filter
            .Where(p => (currentYear - p.Dob.Year) >= minAge && (currentYear - p.Dob.Year) <= maxAge)
            // Health filters
            .Where(p => p.MedicalHistory.ChronicConditions.Count == 0)
            .Where(p => !p.MedicalHistory.Allergies.Any(a => a.SeverityLevel == "anaphylactic"))
            .ToListAsync();

        var compatibleDonors = donors
            .Where(p => IsABloodMatch(p.MedicalHistory.BloodType, bloodType) && IsARhMatch(p.MedicalHistory.Rh, rh))
            .Select(p => new
            {
                Patient = p,
                Score = CalculateTotalScore(p, bloodType, rh, sex, recipientAge)
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Patient)
            .ToList();

        return compatibleDonors;
    }

    private int CalculateTotalScore(Patient pd, BloodType targetBlood, Rh targetRh, Sex targetSex, int recipientAge)
    {
        int score = 0;
        int donorAge = DateTime.Now.Year - pd.Dob.Year;

        // Blood/Rh Score
        if (pd.MedicalHistory.BloodType == targetBlood && pd.MedicalHistory.Rh == targetRh)
            score += 50;
        else
            score += 25;

        // Sex Score
        score += (pd.Sex == targetSex) ? 20 : 10;

        // Age Score
        int ageGap = Math.Abs(donorAge - recipientAge);
        int group = ageGap / 5;
        score += Math.Max(30 - (group * 5), 0);

        return score;
    }

    private static bool IsABloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null)
            return false;
        if (donor == BloodType.O)
            return true;
        if (donor == BloodType.A && (receiver == BloodType.A || receiver == BloodType.AB))
            return true;
        if (donor == BloodType.B && (receiver == BloodType.B || receiver == BloodType.AB))
            return true;
        return (donor == BloodType.AB && receiver == BloodType.AB);
    }

    private static bool IsARhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null)
            return false;
        return donor == Rh.Negative || (donor == Rh.Positive && receiver == Rh.Positive);
    }
}