using HospitalManagement.Data;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public class PatientRepository : IPatientRepository
{
    private readonly EFHospitalDbContext _context;

    public PatientRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public void Add(Patient p) => AddAsync(p).GetAwaiter().GetResult();

    public async Task AddAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);
        _ = await _context.Patients.AddAsync(p);
        _ = await _context.SaveChangesAsync();
    }

    public void Update(Patient p) => UpdateAsync(p).GetAwaiter().GetResult();

    public async Task UpdateAsync(Patient p)
    {
        ArgumentNullException.ThrowIfNull(p);
        _ = _context.Patients.Update(p);
        _ = await _context.SaveChangesAsync();
    }

    public void Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();

    public async Task DeleteAsync(int id)
    {
        Patient? patient = await _context.Patients.FindAsync(id);
        if (patient is not null)
        {
            _ = _context.Patients.Remove(patient);
            _ = await _context.SaveChangesAsync();
        }
    }

    public bool Exists(string cnp) => ExistsAsync(cnp).GetAwaiter().GetResult();

    public Task<bool> ExistsAsync(string cnp) =>
        _context.Patients.AnyAsync(p => p.Cnp == cnp);

    public List<Patient> GetAll(bool include_archived) => GetAllAsync(include_archived).GetAwaiter().GetResult();

    public Task<List<Patient>> GetAllAsync(bool include_archived)
    {
        IQueryable<Patient> query = _context.Patients
            .Include(p => p.MedicalHistory);

        if (!include_archived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return query.AsNoTracking().ToListAsync();
    }

    public List<Patient> GetArchived() => GetArchivedAsync().GetAwaiter().GetResult();

    public Task<List<Patient>> GetArchivedAsync() =>
        _context.Patients
            .AsNoTracking()
            .Where(p => p.IsArchived)
            .ToListAsync();

    public Patient? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();

    public Task<Patient?> GetByIdAsync(int id) =>
        _context.Patients
            .Include(p => p.MedicalHistory)
            .ThenInclude(h => h.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public List<Patient> Search(PatientFilter patientFilter) => SearchAsync(patientFilter).GetAwaiter().GetResult();

    public Task<List<Patient>> SearchAsync(PatientFilter patientFilter)
    {
        ArgumentNullException.ThrowIfNull(patientFilter);

        IQueryable<Patient> query = _context.Patients
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

    public void MarkAsDeceased(int id, DateTime dod) => MarkAsDeceasedAsync(id, dod).GetAwaiter().GetResult();

    public async Task MarkAsDeceasedAsync(int id, DateTime dod)
    {
        Patient? patient = await _context.Patients.FindAsync(id);
        if (patient is not null)
        {
            patient.Dod = dod;
            patient.IsArchived = true;
            _ = await _context.SaveChangesAsync();
        }
    }

    public List<Patient> GetCompatibleDonors(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge) =>
        GetCompatibleDonorsAsync(bloodType, rh, sex, dob, minAge, maxAge).GetAwaiter().GetResult();

    public async Task<List<Patient>> GetCompatibleDonorsAsync(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge)
    {
        int currentYear = DateTime.Now.Year;
        int recipientAge = currentYear - dob.Year;

        List<Patient> donors = await _context.Patients
            .Include(p => p.MedicalHistory)
            .ThenInclude(mh => mh.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .Where(p => !p.IsArchived && p.MedicalHistory != null)
            .Where(p => (currentYear - p.Dob.Year) >= minAge && (currentYear - p.Dob.Year) <= maxAge)
            .Where(p => p.MedicalHistory!.ChronicConditions.Count == 0)
            .Where(p => !p.MedicalHistory!.PatientAllergies.Any(a => a.SeverityLevel.ToLower() == "anaphylactic"))
            .ToListAsync();

        return donors
            .Where(p => IsABloodMatch(p.MedicalHistory?.BloodType, bloodType) && IsARhMatch(p.MedicalHistory?.Rh, rh))
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

        if (donor.MedicalHistory?.BloodType == targetBlood && donor.MedicalHistory.Rh == targetRh)
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

        return donor == Rh.Negative || (donor == Rh.Positive && receiver == Rh.Positive);
    }
}
