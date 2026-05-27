using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class TriageParametersRepository : ITriageParametersRepository
{
    private readonly EFHospitalDbContext context;

    public TriageParametersRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public Task<List<Triage_Parameters>> GetAllAsync() =>
        context.TriageParameters.AsNoTracking().ToListAsync();

    public Task<Triage_Parameters?> GetByIdAsync(int id) =>
        context.TriageParameters.AsNoTracking().FirstOrDefaultAsync(p => p.TriageId == id);

    public async Task<Triage_Parameters> CreateAsync(Triage_Parameters parameters)
    {
        parameters.ValidateParameters();

        Triage_Parameters? existingParameters = await context.TriageParameters
            .FirstOrDefaultAsync(p => p.TriageId == parameters.TriageId);
        if (existingParameters is not null)
        {
            existingParameters.Consciousness = parameters.Consciousness;
            existingParameters.Breathing = parameters.Breathing;
            existingParameters.Bleeding = parameters.Bleeding;
            existingParameters.Injury_Type = parameters.Injury_Type;
            existingParameters.Pain_Level = parameters.Pain_Level;
            await context.SaveChangesAsync();
            return existingParameters;
        }

        await context.TriageParameters.AddAsync(parameters);
        await context.SaveChangesAsync();
        return parameters;
    }

    public async Task<bool> UpdateAsync(int id, Triage_Parameters parameters)
    {
        Triage_Parameters? existingParameters = await context.TriageParameters.FirstOrDefaultAsync(p => p.TriageId == id);
        if (existingParameters is null)
        {
            return false;
        }

        existingParameters.Consciousness = parameters.Consciousness;
        existingParameters.Breathing = parameters.Breathing;
        existingParameters.Bleeding = parameters.Bleeding;
        existingParameters.Injury_Type = parameters.Injury_Type;
        existingParameters.Pain_Level = parameters.Pain_Level;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Triage_Parameters? parameters = await context.TriageParameters.FirstOrDefaultAsync(p => p.TriageId == id);
        if (parameters is null)
        {
            return false;
        }

        context.TriageParameters.Remove(parameters);
        await context.SaveChangesAsync();
        return true;
    }
}
