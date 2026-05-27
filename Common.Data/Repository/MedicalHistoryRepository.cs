using Common.Data.Entity;
using Common.Data.Data;
using Common.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class MedicalHistoryRepository : IMedicalHistoryRepository
{
    private readonly EFHospitalDbContext context;

    public MedicalHistoryRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public int Create(MedicalHistory history) => CreateAsync(history).GetAwaiter().GetResult();

    public async Task<int> CreateAsync(MedicalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        _ = context.MedicalHistory.Add(history);
        _ = await context.SaveChangesAsync();
        return history.Id;
    }

    public void Update(MedicalHistory history) => UpdateAsync(history).GetAwaiter().GetResult();

    public async Task UpdateAsync(MedicalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        MedicalHistory existing = await context.MedicalHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == history.Id)
            ?? throw new KeyNotFoundException($"Medical history {history.Id} was not found.");

        if (existing.PatientId != history.PatientId)
        {
            throw new InvalidOperationException("Cannot reassign medical history to another patient.");
        }

        _ = context.MedicalHistory.Update(history);
        _ = await context.SaveChangesAsync();
    }

    public MedicalHistory? GetByPatientId(int patientId) => GetByPatientIdAsync(patientId).GetAwaiter().GetResult();

    public Task<MedicalHistory?> GetByPatientIdAsync(int patientId) =>
        context.MedicalHistory
            .Include(h => h.Patient)
            .Include(h => h.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.PatientId == patientId);

    public MedicalHistory? GetById(int historyId) => GetByIdAsync(historyId).GetAwaiter().GetResult();

    public Task<MedicalHistory?> GetByIdAsync(int historyId) =>
        context.MedicalHistory
            .Include(h => h.Patient)
            .Include(h => h.PatientAllergies)
            .ThenInclude(pa => pa.Allergy)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == historyId);

    public void SaveAllergies(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies) =>
        SaveAllergiesAsync(historyId, allergies).GetAwaiter().GetResult();

    public async Task SaveAllergiesAsync(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies)
    {
        if (allergies is null || allergies.Count == 0)
        {
            return;
        }

        foreach ((Allergy allergy, string severity) in allergies)
        {
            context.PatientAllergies.Add(new PatientAllergy
            {
                MedicalHistoryId = historyId,
                AllergyId = allergy.Id,
                Allergy = allergy,
                SeverityLevel = severity,
            });
        }

        _ = await context.SaveChangesAsync();
    }

    public List<string> GetChronicConditions(int historyId) => GetChronicConditionsAsync(historyId).GetAwaiter().GetResult();

    public async Task<List<string>> GetChronicConditionsAsync(int historyId)
    {
        List<string>? conditions = await context.MedicalHistory
            .Where(h => h.Id == historyId)
            .Select(h => h.ChronicConditions)
            .FirstOrDefaultAsync();

        return conditions ?? new List<string>();
    }

    public List<(Allergy Allergy, string SeverityLevel)> GetAllergiesByHistoryId(int historyId) =>
        GetAllergiesByHistoryIdAsync(historyId).GetAwaiter().GetResult();

    public async Task<List<(Allergy Allergy, string SeverityLevel)>> GetAllergiesByHistoryIdAsync(int historyId)
    {
        List<PatientAllergy> entries = await context.PatientAllergies
            .Include(pa => pa.Allergy)
            .Where(pa => pa.MedicalHistoryId == historyId)
            .AsNoTracking()
            .ToListAsync();

        return entries.Select(pa => (pa.Allergy, pa.SeverityLevel)).ToList();
    }
}
