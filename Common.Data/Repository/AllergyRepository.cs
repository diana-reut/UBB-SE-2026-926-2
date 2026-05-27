using Common.Data.Entity;
using Common.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class AllergyRepository : IAllergyRepository
{
    private readonly EFHospitalDbContext context;

    public AllergyRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public Task<List<Allergy>> GetAllergiesAsync() => context.Allergies.AsNoTracking().ToListAsync();

    public Allergy? GetById(int id) =>
        context.Allergies
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);

    public Task<Allergy?> GetByIdAsync(int id) =>
        context.Allergies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
}
