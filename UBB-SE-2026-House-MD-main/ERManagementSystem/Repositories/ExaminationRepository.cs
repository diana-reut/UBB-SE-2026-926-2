using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ERManagementSystem.Repositories
{
    public class ExaminationRepository : IExaminationRepository
    {
        private readonly EFHospitalDbContext context;

        public ExaminationRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(Examination exam)
            => AddAsync(exam).GetAwaiter().GetResult();

        public async Task AddAsync(Examination exam)
        {
            await context.AddAsync(exam);
            await context.SaveChangesAsync();
            Logger.Info($"Successfully added new examination record for Visit {exam.Visit_ID}.");
        }

        public List<Examination> GetByPatientId(string patientId)
            => GetByPatientIdAsync(patientId).GetAwaiter().GetResult();

        public Task<List<Examination>> GetByPatientIdAsync(string patientId)
        {
            return context.Examinations
                .Join(
                    context.ERVisits,
                    exam => exam.Visit_ID,
                    visit => visit.Visit_ID,
                    (exam, visit) => new { exam, visit })
                .Where(x => x.visit.Patient_ID == patientId)
                .OrderByDescending(x => x.exam.Exam_Time)
                .Select(x => x.exam)
                .AsNoTracking()
                .ToListAsync();
        }

        public void UpdateNotes(int examId, string notes)
            => UpdateNotesAsync(examId, notes).GetAwaiter().GetResult();

        public async Task UpdateNotesAsync(int examId, string notes)
        {
            Examination exam = await context.Examinations.FirstAsync(e => e.Exam_ID == examId);
            exam.Notes = notes;
            await context.SaveChangesAsync();
        }

        public ExaminationSummaryDTO? GetExaminationSummary(int examId)
            => GetExaminationSummaryAsync(examId).GetAwaiter().GetResult();

        public Task<ExaminationSummaryDTO?> GetExaminationSummaryAsync(int examId)
        {
            return (
                from exam in context.Examinations
                join visit in context.ERVisits on exam.Visit_ID equals visit.Visit_ID
                join patient in context.Patients on visit.Patient_ID equals patient.Cnp
                join triage in context.Triages on visit.Visit_ID equals triage.Visit_ID
                join parameters in context.TriageParameters on triage.Triage_ID equals parameters.Triage_ID
                where exam.Exam_ID == examId
                select new ExaminationSummaryDTO
                {
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    ArrivalDateTime = visit.Arrival_date_time,
                    ChiefComplaint = visit.Chief_Complaint,
                    TriageLevel = triage.Triage_Level,
                    Specialization = triage.Specialization,
                    Consciousness = parameters.Consciousness,
                    Breathing = parameters.Breathing,
                    Bleeding = parameters.Bleeding,
                    InjuryType = parameters.Injury_Type,
                    PainLevel = parameters.Pain_Level,
                    DoctorId = exam.Doctor_ID,
                    ExamTime = exam.Exam_Time,
                    Notes = exam.Notes,
                }).FirstOrDefaultAsync();
        }

        public int GetFirstRoomId() => GetFirstRoomIdAsync().GetAwaiter().GetResult();

        public async Task<int> GetFirstRoomIdAsync()
        {
            return await context.ERRooms
                .OrderBy(r => r.Room_ID)
                .Select(r => r.Room_ID)
                .FirstOrDefaultAsync();
        }
    }
}
