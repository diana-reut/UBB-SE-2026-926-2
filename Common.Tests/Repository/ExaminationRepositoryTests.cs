using Common.Data.Data;
using Common.Data.Models;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Repository;

[TestClass]
public sealed class ExaminationRepositoryTests
{
    private static EFHospitalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EFHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Examination MakeExamination(int id = 1, string notes = "Stable") => new()
    {
        Exam_ID = id,
        Visit_ID = id * 10,
        Doctor_ID = 7,
        Room_ID = 5,
        Notes = notes,
        Exam_Time = new DateTime(2026, 1, id)
    };

    [TestMethod]
    public async Task GetAllAsync_WhenExaminationsExist_ReturnsAllItems()
    {
        await using var context = CreateContext();
        context.Examinations.AddRange(MakeExamination(1), MakeExamination(2));
        await context.SaveChangesAsync();
        var sut = new ExaminationRepository(context);

        List<Examination> result = await sut.GetAllAsync();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenExaminationExists_ReturnsMatchingExamination()
    {
        await using var context = CreateContext();
        context.Examinations.Add(MakeExamination(8, "Needs observation"));
        await context.SaveChangesAsync();
        var sut = new ExaminationRepository(context);

        Examination? result = await sut.GetByIdAsync(8);

        Assert.AreEqual("Needs observation", result!.Notes);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenExaminationExists_UpdatesDoctorId()
    {
        await using var context = CreateContext();
        context.Examinations.Add(MakeExamination(8));
        await context.SaveChangesAsync();
        var sut = new ExaminationRepository(context);

        await sut.UpdateAsync(8, new Examination { Exam_ID = 99, Visit_ID = 80, Doctor_ID = 11, Room_ID = 5, Notes = "Updated", Exam_Time = DateTime.Today });

        Assert.AreEqual(11, context.Examinations.Single().Doctor_ID);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenExaminationDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var sut = new ExaminationRepository(context);

        bool result = await sut.DeleteAsync(5);

        Assert.IsFalse(result);
    }
}
