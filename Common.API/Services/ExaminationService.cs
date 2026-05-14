using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Models;
using Common.Data.Repository;

namespace Common.API.Services;

public class ExaminationService : IExaminationService
{
    private readonly IExaminationRepository repository;
    private readonly IERVisitRepository erVisitRepository;
    private readonly IERRoomRepository erRoomRepository;
    private readonly ITriageRepository triageRepository;
    private readonly ITriageParametersRepository triageParametersRepository;
    private readonly IPatientRepository patientRepository;

    public ExaminationService(
        IExaminationRepository repository,
        IERVisitRepository erVisitRepository,
        IERRoomRepository erRoomRepository,
        ITriageRepository triageRepository,
        ITriageParametersRepository triageParametersRepository,
        IPatientRepository patientRepository)
    {
        this.repository = repository;
        this.erVisitRepository = erVisitRepository;
        this.erRoomRepository = erRoomRepository;
        this.triageRepository = triageRepository;
        this.triageParametersRepository = triageParametersRepository;
        this.patientRepository = patientRepository;
    }

    public Task<List<Examination>> GetAllAsync() =>
        repository.GetAllAsync();

    public Task<Examination?> GetByIdAsync(int id) =>
        repository.GetByIdAsync(id);

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
    {
        return (await repository.GetAllAsync())
            .Where(examination => examination.Visit_ID == visitId)
            .OrderByDescending(examination => examination.Exam_Time)
            .ToList();
    }

    public Task<Examination> CreateAsync(Examination examination) =>
        repository.CreateAsync(examination);

    public Task<bool> UpdateAsync(int id, Examination examination) =>
        repository.UpdateAsync(id, examination);

    public Task<bool> DeleteAsync(int id) =>
        repository.DeleteAsync(id);

    public async Task<List<ER_Visit>> GetEligibleVisitsAsync()
    {
        HashSet<int> roomLinkedVisitIds = (await erRoomRepository.GetAllAsync())
            .Where(room => room.Current_Visit_ID.HasValue)
            .Select(room => room.Current_Visit_ID!.Value)
            .ToHashSet();

        List<ER_Visit> inRoom = (await erVisitRepository.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase))
            .ToList();

        List<ER_Visit> waiting = (await erVisitRepository.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return inRoom
            .Concat(waiting)
            .Where(visit => !string.Equals(visit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                || roomLinkedVisitIds.Contains(visit.Visit_ID))
            .OrderBy(visit => visit.Arrival_date_time)
            .ToList();
    }

    public async Task<List<Examination>> GetPatientHistoryAsync(string patientId)
    {
        HashSet<int> patientVisitIds = (await erVisitRepository.GetAllAsync())
            .Where(visit => visit.Patient_ID == patientId)
            .Select(visit => visit.Visit_ID)
            .ToHashSet();

        if (patientVisitIds.Count == 0)
        {
            return new List<Examination>();
        }

        return (await repository.GetAllAsync())
            .Where(examination => patientVisitIds.Contains(examination.Visit_ID))
            .OrderByDescending(examination => examination.Exam_Time)
            .ToList();
    }

    public async Task<ERExaminationSummaryDto?> GetSummaryByVisitIdAsync(int visitId)
    {
        Examination? examination = (await repository.GetAllAsync())
            .Where(item => item.Visit_ID == visitId)
            .OrderByDescending(item => item.Exam_Time)
            .FirstOrDefault();
        if (examination == null)
        {
            return null;
        }

        ER_Visit? visit = await erVisitRepository.GetByIdAsync(visitId);
        if (visit == null)
        {
            return null;
        }

        Patient? patient = (await patientRepository.SearchAsync(new PatientFilter { CNP = visit.Patient_ID }))
            .FirstOrDefault();
        Triage? triage = (await triageRepository.GetAllAsync())
            .FirstOrDefault(item => item.Visit_ID == visit.Visit_ID);
        if (patient == null || triage == null)
        {
            return null;
        }

        Triage_Parameters? parameters = (await triageParametersRepository.GetAllAsync())
            .FirstOrDefault(item => item.Triage_ID == triage.Triage_ID);
        if (parameters == null)
        {
            return null;
        }

        return new ERExaminationSummaryDto
        {
            FirstName = patient.First_Name,
            LastName = patient.Last_Name,
            ArrivalDateTime = visit.Arrival_date_time,
            ChiefComplaint = visit.Chief_Complaint,
            TriageLevel = triage.Triage_Level,
            Specialization = triage.Specialization,
            Consciousness = parameters.Consciousness,
            Breathing = parameters.Breathing,
            Bleeding = parameters.Bleeding,
            InjuryType = parameters.Injury_Type,
            PainLevel = parameters.Pain_Level,
            DoctorId = examination.Doctor_ID,
            ExamTime = examination.Exam_Time,
            Notes = examination.Notes
        };
    }
}
