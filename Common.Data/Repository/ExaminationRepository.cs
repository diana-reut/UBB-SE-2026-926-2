using Common.Data.Data;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class ExaminationRepository : IExaminationRepository
{
    private readonly EFHospitalDbContext context;

    public ExaminationRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public Task<List<Examination>> GetAllAsync() =>
        context.Examinations.AsNoTracking().ToListAsync();

    public Task<Examination?> GetByIdAsync(int id) =>
        context.Examinations.AsNoTracking().FirstOrDefaultAsync(e => e.Exam_ID == id);

    public async Task<Examination> CreateAsync(Examination examination)
    {
        await context.Examinations.AddAsync(examination);
        await context.SaveChangesAsync();
        return examination;
    }

    public async Task<bool> UpdateAsync(int id, Examination examination)
    {
        Examination? existingExamination = await context.Examinations.FirstOrDefaultAsync(e => e.Exam_ID == id);
        if (existingExamination is null)
        {
            return false;
        }

        existingExamination.Visit_ID = examination.Visit_ID;
        existingExamination.Doctor_ID = examination.Doctor_ID;
        existingExamination.Exam_Time = examination.Exam_Time;
        existingExamination.Room_ID = examination.Room_ID;
        existingExamination.Notes = examination.Notes;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Examination? examination = await context.Examinations.FirstOrDefaultAsync(e => e.Exam_ID == id);
        if (examination is null)
        {
            return false;
        }

        context.Examinations.Remove(examination);
        await context.SaveChangesAsync();
        return true;
    }
}
