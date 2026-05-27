using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class TransplantRepository : ITransplantRepository
{
    private readonly EFHospitalDbContext context;

    public TransplantRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public void Add(Transplant transplant) => AddAsync(transplant).GetAwaiter().GetResult();

    public async Task AddAsync(Transplant transplant)
    {
        await context.Transplants.AddAsync(transplant);
        await context.SaveChangesAsync();
    }

    public Task<List<Transplant>> GetAllAsync() =>
        context.Transplants.AsNoTracking().ToListAsync();

    public List<Transplant> GetWaitingByOrgan(string organType) => GetWaitingByOrganAsync(organType).GetAwaiter().GetResult();

    public Task<List<Transplant>> GetWaitingByOrganAsync(string organType)
    {
        List<string> acceptableOrganTypes = ExpandOrganAliases(organType);

        return context.Transplants
            .Where(t => acceptableOrganTypes.Contains(t.OrganType) && t.Status == TransplantStatus.Pending)
            .OrderBy(t => t.RequestDate)
            .ToListAsync();
    }

    public void Update(int id, int donorId, float score) => UpdateAsync(id, donorId, score).GetAwaiter().GetResult();

    public async Task UpdateAsync(int id, int donorId, float score)
    {
        Transplant transplant = await context.Transplants.FirstAsync(t => t.TransplantId == id);
        transplant.DonorId = donorId;
        transplant.Status = TransplantStatus.Scheduled;
        transplant.CompatibilityScore = score;
        await context.SaveChangesAsync();
    }

    public List<Transplant> GetTopMatches(string organType) => GetTopMatchesAsync(organType).GetAwaiter().GetResult();

    public Task<List<Transplant>> GetTopMatchesAsync(string organType)
    {
        List<string> acceptableOrganTypes = ExpandOrganAliases(organType);

        return context.Transplants
            .Where(t => acceptableOrganTypes.Contains(t.OrganType) && t.Status == TransplantStatus.Pending)
            .OrderByDescending(t => t.CompatibilityScore)
            .Take(5)
            .ToListAsync();
    }

    public List<Transplant> GetByReceiverId(int receiverId) => GetByReceiverIdAsync(receiverId).GetAwaiter().GetResult();

    public Task<List<Transplant>> GetByReceiverIdAsync(int receiverId) =>
        context.Transplants.Where(t => t.ReceiverId == receiverId).ToListAsync();

    public List<Transplant> GetByDonorId(int donorId) => GetByDonorIdAsync(donorId).GetAwaiter().GetResult();

    public Task<List<Transplant>> GetByDonorIdAsync(int donorId) =>
        context.Transplants.Where(t => t.DonorId == donorId).ToListAsync();

    public Transplant? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();

    public Task<Transplant?> GetByIdAsync(int id) =>
        context.Transplants.AsNoTracking().FirstOrDefaultAsync(t => t.TransplantId == id);

    public async Task<bool> UpdateAsync(int id, Transplant transplant)
    {
        Transplant? existingTransplant = await context.Transplants.FirstOrDefaultAsync(t => t.TransplantId == id);
        if (existingTransplant is null)
        {
            return false;
        }

        existingTransplant.ReceiverId = transplant.ReceiverId;
        existingTransplant.DonorId = transplant.DonorId;
        existingTransplant.OrganType = transplant.OrganType;
        existingTransplant.RequestDate = transplant.RequestDate;
        existingTransplant.TransplantDate = transplant.TransplantDate;
        existingTransplant.Status = transplant.Status;
        existingTransplant.CompatibilityScore = transplant.CompatibilityScore;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Transplant? transplant = await context.Transplants.FirstOrDefaultAsync(t => t.TransplantId == id);
        if (transplant is null)
        {
            return false;
        }

        context.Transplants.Remove(transplant);
        await context.SaveChangesAsync();
        return true;
    }

    private static List<string> ExpandOrganAliases(string organType)
    {
        string normalized = organType.Trim();

        return normalized switch
        {
            "Lung" or "Lungs" =>["Lung", "Lungs"],
            _ =>[normalized],
        };
    }
}
