using HospitalManagement.Data;
using HospitalManagement.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal class AllergyRepository : IAllergyRepository
{
    private readonly EFHospitalDbContext _context;

    public AllergyRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Allergy>> GetAllergiesAsync()
    {
        return await _context.Allergies
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Allergy?> GetByIdAsync(int id)
    {
        return _context.Allergies
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
