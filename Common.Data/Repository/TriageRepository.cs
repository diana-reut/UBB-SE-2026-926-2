using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class TriageRepository : ITriageRepository
{
    private readonly EFHospitalDbContext _context;

    public TriageRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<Triage>> GetAllAsync() =>
        _context.Triages.AsNoTracking().ToListAsync();

    public Task<Triage?> GetByIdAsync(int id) =>
        _context.Triages.AsNoTracking().FirstOrDefaultAsync(t => t.Triage_ID == id);

    public async Task<Triage> CreateAsync(Triage triage)
    {
        await _context.Triages.AddAsync(triage);
        await _context.SaveChangesAsync();
        return triage;
    }

    public async Task<bool> UpdateAsync(int id, Triage triage)
    {
        Triage? existingTriage = await _context.Triages.FirstOrDefaultAsync(t => t.Triage_ID == id);
        if (existingTriage is null)
        {
            return false;
        }

        existingTriage.Visit_ID = triage.Visit_ID;
        existingTriage.Triage_Level = triage.Triage_Level;
        existingTriage.Specialization = triage.Specialization;
        existingTriage.Nurse_ID = triage.Nurse_ID;
        existingTriage.Triage_Time = triage.Triage_Time;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Triage? triage = await _context.Triages.FirstOrDefaultAsync(t => t.Triage_ID == id);
        if (triage is null)
        {
            return false;
        }

        _context.Triages.Remove(triage);
        await _context.SaveChangesAsync();
        return true;
    }
}
