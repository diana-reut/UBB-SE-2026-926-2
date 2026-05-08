using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class ERVisitRepository : IERVisitRepository
{
    private readonly EFHospitalDbContext _context;

    public ERVisitRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<ER_Visit>> GetAllAsync() =>
        _context.ERVisits.AsNoTracking().ToListAsync();

    public Task<ER_Visit?> GetByIdAsync(int id) =>
        _context.ERVisits.AsNoTracking().FirstOrDefaultAsync(v => v.Visit_ID == id);

    public async Task<ER_Visit> CreateAsync(ER_Visit visit)
    {
        await _context.ERVisits.AddAsync(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task<bool> UpdateAsync(int id, ER_Visit visit)
    {
        ER_Visit? existingVisit = await _context.ERVisits.FirstOrDefaultAsync(v => v.Visit_ID == id);
        if (existingVisit is null)
        {
            return false;
        }

        existingVisit.Patient_ID = visit.Patient_ID;
        existingVisit.Arrival_date_time = visit.Arrival_date_time;
        existingVisit.Chief_Complaint = visit.Chief_Complaint;
        existingVisit.Status = visit.Status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ER_Visit? visit = await _context.ERVisits.FirstOrDefaultAsync(v => v.Visit_ID == id);
        if (visit is null)
        {
            return false;
        }

        _context.ERVisits.Remove(visit);
        await _context.SaveChangesAsync();
        return true;
    }
}
