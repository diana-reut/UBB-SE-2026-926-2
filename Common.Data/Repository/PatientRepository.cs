using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Integration;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class PatientRepository : IPatientRepository
{
    private readonly EFHospitalDbContext context;

    public PatientRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);
        _ = await context.Patients.AddAsync(p);
        _ = await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);

        Patient? trackedPatient = context.Patients.Local.FirstOrDefault(existing => existing.Id == p.Id);
        if (trackedPatient is not null)
        {
            context.Entry(trackedPatient).CurrentValues.SetValues(p);
        }
        else
        {
            _ = context.Patients.Update(p);
        }

        _ = await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Patient? patient = await context.Patients.FindAsync(id);
        if (patient is not null)
        {
            _ = context.Patients.Remove(patient);
            _ = await context.SaveChangesAsync();
        }
    }

    public Task<bool> ExistsAsync(string cnp) =>
        context.Patients.AnyAsync(p => p.Cnp == cnp);

    public Task<List<Patient>> GetAllAsync(bool include_archived)
    {
        IQueryable<Patient> query = context.Patients
            .Include(p => p.MedicalHistory);

        if (!include_archived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return query.AsNoTracking().ToListAsync();
    }

    public Task<List<Patient>> GetArchivedAsync() =>
        context.Patients
            .AsNoTracking()
            .Where(p => p.IsArchived)
            .ToListAsync();

    public Task<Patient?> GetByIdAsync(int id) =>
        context.Patients
            .Include(p => p.MedicalHistory)
            .ThenInclude(h => h.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<Patient>> SearchAsync(PatientFilter patientFilter)
    {
        ArgumentNullException.ThrowIfNull(patientFilter);

        IQueryable<Patient> query = context.Patients
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

        return query.AsNoTracking().ToListAsync();
    }

    public async Task MarkAsDeceasedAsync(int id, DateTime dod)
    {
        Patient? patient = await context.Patients.FindAsync(id);
        if (patient is not null)
        {
            patient.Dod = dod;
            patient.IsArchived = true;
            _ = await context.SaveChangesAsync();
        }
    }

    public async Task<List<Patient>> GetCompatibleDonorsAsync(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge)
    {
        int currentYear = DateTime.Now.Year;
        int recipientAge = currentYear - dob.Year;

        List<Patient> donors = await context.Patients
            .Include(p => p.MedicalHistory)
            .ThenInclude(mh => mh.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .Where(p => !p.IsArchived && p.MedicalHistory != null)
            .Where(p => (currentYear - p.Dob.Year) >= minAge && (currentYear - p.Dob.Year) <= maxAge)
            .Where(p => p.MedicalHistory!.ChronicConditions.Count == 0)
            .Where(p => !p.MedicalHistory!.PatientAllergies.Any(a => a.SeverityLevel.ToLower() == "anaphylactic"))
            .ToListAsync();

        return donors
            .Where(p => IsABloodMatch(p.MedicalHistory!.BloodType, bloodType) && IsARhMatch(p.MedicalHistory.Rh, rh))
            .Select(p => new
            {
                Patient = p,
                Score = CalculateTotalScore(p, bloodType, rh, sex, recipientAge),
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Patient)
            .ToList();
    }

    private int CalculateTotalScore(Patient donor, BloodType targetBlood, Rh targetRh, Sex targetSex, int recipientAge)
    {
        int score = 0;
        int donorAge = DateTime.Now.Year - donor.Dob.Year;

        if (donor.MedicalHistory!.BloodType == targetBlood && donor.MedicalHistory.Rh == targetRh)
        {
            score += 50;
        }
        else
        {
            score += 25;
        }

        score += donor.Sex == targetSex ? 20 : 10;

        int ageGap = Math.Abs(donorAge - recipientAge);
        int group = ageGap / 5;
        score += Math.Max(30 - (group * 5), 0);

        return score;
    }

    private static bool IsABloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null)
        {
            return false;
        }

        if (donor == BloodType.O)
        {
            return true;
        }

        if (donor == BloodType.A && (receiver == BloodType.A || receiver == BloodType.AB))
        {
            return true;
        }

        if (donor == BloodType.B && (receiver == BloodType.B || receiver == BloodType.AB))
        {
            return true;
        }

        return donor == BloodType.AB && receiver == BloodType.AB;
    }

    private static bool IsARhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null)
        {
            return false;
        }

        if (donor == Rh.Negative)
        {
            return true;
        }

        return receiver == Rh.Positive;
    }
}
