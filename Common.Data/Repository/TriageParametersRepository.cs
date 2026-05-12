using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class TriageParametersRepository : ITriageParametersRepository
{
    private readonly EFHospitalDbContext _context;

    public TriageParametersRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<Triage_Parameters>> GetAllAsync() =>
        _context.TriageParameters.AsNoTracking().ToListAsync();

    public Task<Triage_Parameters?> GetByIdAsync(int id) =>
        _context.TriageParameters.AsNoTracking().FirstOrDefaultAsync(p => p.Triage_ID == id);

    public async Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters)
    {
        await _context.TriageParameters.AddAsync(parameters);
        await _context.SaveChangesAsync();
        return parameters;
    }

    public async Task<bool> UpdateAsync(int id, Triage_Parameters parameters)
    {
        Triage_Parameters? existingParameters = await _context.TriageParameters.FirstOrDefaultAsync(p => p.Triage_ID == id);
        if (existingParameters is null)
        {
            return false;
        }

        existingParameters.Consciousness = parameters.Consciousness;
        existingParameters.Breathing = parameters.Breathing;
        existingParameters.Bleeding = parameters.Bleeding;
        existingParameters.Injury_Type = parameters.Injury_Type;
        existingParameters.Pain_Level = parameters.Pain_Level;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Triage_Parameters? parameters = await _context.TriageParameters.FirstOrDefaultAsync(p => p.Triage_ID == id);
        if (parameters is null)
        {
            return false;
        }

        _context.TriageParameters.Remove(parameters);
        await _context.SaveChangesAsync();
        return true;
    }
}
