using Common.Data.Entity;
using Common.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class AllergyRepository : IAllergyRepository
{
    private readonly EFHospitalDbContext _context;

    public AllergyRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<Allergy>> GetAllergiesAsync() => _context.Allergies.AsNoTracking().ToListAsync();

    public Allergy? GetById(int id) =>
        _context.Allergies
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);

    public Task<Allergy?> GetByIdAsync(int id) =>
        _context.Allergies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
}
