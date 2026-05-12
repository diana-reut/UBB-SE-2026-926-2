using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class ExaminationRepository : IExaminationRepository
{
    private readonly EFHospitalDbContext _context;

    public ExaminationRepository(EFHospitalDbContext context)
    {
        _context = context;
    }

    public Task<List<Examination>> GetAllAsync() =>
        _context.Examinations.AsNoTracking().ToListAsync();

    public Task<Examination?> GetByIdAsync(int id) =>
        _context.Examinations.AsNoTracking().FirstOrDefaultAsync(e => e.Exam_ID == id);

    public async Task<Examination> CreateAsync(Examination examination)
    {
        await _context.Examinations.AddAsync(examination);
        await _context.SaveChangesAsync();
        return examination;
    }

    public async Task<bool> UpdateAsync(int id, Examination examination)
    {
        Examination? existingExamination = await _context.Examinations.FirstOrDefaultAsync(e => e.Exam_ID == id);
        if (existingExamination is null)
        {
            return false;
        }

        existingExamination.Visit_ID = examination.Visit_ID;
        existingExamination.Doctor_ID = examination.Doctor_ID;
        existingExamination.Exam_Time = examination.Exam_Time;
        existingExamination.Room_ID = examination.Room_ID;
        existingExamination.Notes = examination.Notes;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Examination? examination = await _context.Examinations.FirstOrDefaultAsync(e => e.Exam_ID == id);
        if (examination is null)
        {
            return false;
        }

        _context.Examinations.Remove(examination);
        await _context.SaveChangesAsync();
        return true;
    }
}
