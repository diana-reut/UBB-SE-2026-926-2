using Common.Data.Entity;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class AllergyRepository : IAllergyRepository
{
    private readonly EFHospitalDbContext _context;

    public AllergyRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Allergy> GetAllergies() =>
        _context.Allergies
            .AsNoTracking()
            .ToList();

    public Task<IEnumerable<Allergy>> GetAllergiesAsync() =>
        Task.FromResult(GetAllergies());

    public Allergy? GetById(int id) =>
        _context.Allergies
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);

    public Task<Allergy?> GetByIdAsync(int id) =>
        _context.Allergies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
}
