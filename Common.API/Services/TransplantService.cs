using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Repository;

namespace Common.API.Services;

public class TransplantService : ITransplantService
{
    private readonly ITransplantRepository transplantRepository;
    private readonly IPatientRepository patientRepository;
    private readonly IMedicalRecordRepository recordRepository;
    private readonly IBloodCompatibilityService compatibilityService;
    private readonly IMedicalHistoryRepository historyRepository;

    private const int MaxScoreModifier = 20;
    private const int MinScoreModifier = 5;
    private const int ComparativeERVisits = 10;
    private const int TimeIntervalMonths = 3;

    public TransplantService(
        ITransplantRepository transplantRepository,
        IPatientRepository patientRepository,
        IMedicalRecordRepository recordRepository,
        IBloodCompatibilityService compatibilityService,
        IMedicalHistoryRepository historyRepository)
    {
        this.transplantRepository = transplantRepository;
        this.patientRepository = patientRepository;
        this.recordRepository = recordRepository;
        this.compatibilityService = compatibilityService;
        this.historyRepository = historyRepository;
    }

    public Task<List<Transplant>> GetAllAsync() =>
        transplantRepository.GetAllAsync();

    public async Task<Transplant> CreateAsync(Transplant transplant)
    {
        await transplantRepository.AddAsync(transplant);
        return transplant;
    }

    public Task<bool> UpdateAsync(int id, Transplant transplant) =>
        transplantRepository.UpdateAsync(id, transplant);

    public Task<bool> DeleteAsync(int id) =>
        transplantRepository.DeleteAsync(id);

    public Task<Transplant?> GetByIdAsync(int id)
    {
        return transplantRepository.GetByIdAsync(id);
    }

    public Task<List<Transplant>> GetByReceiverIdAsync(int receiverId)
    {
        return transplantRepository.GetByReceiverIdAsync(receiverId);
    }

    public Task<List<Transplant>> GetByDonorIdAsync(int donorId)
    {
        return transplantRepository.GetByDonorIdAsync(donorId);
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
    {
        _ = await patientRepository.GetByIdAsync(receiverId) ?? throw new ArgumentException("Receiver not found.");

        string normalizedOrganType = NormalizeOrganType(organType);

        var request = new Transplant
        {
            ReceiverId = receiverId,
            DonorId = null,
            OrganType = normalizedOrganType,
            RequestDate = DateTime.UtcNow,
            Status = Common.Data.Entity.Enums.TransplantStatus.Pending,
            CompatibilityScore = 0,
        };

        await transplantRepository.AddAsync(request);
    }

    public Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
    {
        return transplantRepository.UpdateAsync(transplantId, donorId, finalScore);
    }

    public async Task<List<Transplant>> GetTopMatchesForDonorAsync(int donorId, string organType)
    {
        string normalizedOrganType = NormalizeOrganType(organType);
        Patient? donor = await patientRepository.GetByIdAsync(donorId);

        if (donor?.IsDeceased != true || !donor.IsDonor)
        {
            throw new InvalidOperationException("Donor must be deceased and registered.");
        }

        donor.MedicalHistory = await historyRepository.GetByPatientIdAsync(donor.Id);

        List<Transplant> waitlist = await transplantRepository.GetWaitingByOrganAsync(normalizedOrganType);
        var scoredMatches = new List<Transplant>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = await patientRepository.GetByIdAsync(request.ReceiverId);
            if (receiver is null)
            {
                continue;
            }

            receiver.MedicalHistory = await historyRepository.GetByPatientIdAsync(receiver.Id);

            if (receiver.MedicalHistory?.BloodType is null || receiver.MedicalHistory.Rh is null)
            {
                continue;
            }

            if (!compatibilityService.IsBloodMatch(donor.MedicalHistory?.BloodType, receiver.MedicalHistory.BloodType.Value))
            {
                continue;
            }

            if (!compatibilityService.IsRhMatch(donor.MedicalHistory?.Rh, receiver.MedicalHistory.Rh.Value))
            {
                continue;
            }

            request.CompatibilityScore = await CalculatePostMortemScoreAsync(donor, receiver);
            scoredMatches.Add(request);
        }

        return scoredMatches
            .OrderByDescending(t => t.CompatibilityScore)
            .ThenBy(t => t.RequestDate)
            .Take(5)
            .ToList();
    }

    public async Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType)
    {
        List<Transplant> matches = await GetTopMatchesForDonorAsync(donorId, organType);
        var result = new List<TransplantMatch>();

        foreach (Transplant transplant in matches)
        {
            Patient? receiver = await patientRepository.GetByIdAsync(transplant.ReceiverId);
            MedicalHistory? receiverHistory = receiver is not null ? await historyRepository.GetByPatientIdAsync(receiver.Id) : null;
            string receiverName = receiver is not null ? $"{receiver.FirstName} {receiver.LastName}" : "Unknown";
            string bloodType = receiverHistory?.BloodType?.ToString() ?? "Unknown";

            result.Add(new TransplantMatch
            {
                TransplantId = transplant.TransplantId,
                ReceiverId = transplant.ReceiverId,
                ReceiverName = receiverName,
                BloodType = bloodType,
                CompatibilityScore = transplant.CompatibilityScore,
                RequestDate = transplant.RequestDate,
                WaitingDays = (DateTime.UtcNow - transplant.RequestDate).Days,
            });
        }

        return result;
    }

    public async Task<bool> IsUrgentAsync(int patientId)
    {
        DateTime threeMonthsAgo = DateTime.UtcNow.AddMonths(-TimeIntervalMonths);
        int erVisits = await recordRepository.GetERVisitCountAsync(patientId, threeMonthsAgo);
        return erVisits >= ComparativeERVisits;
    }

    public async Task<string?> GetChronicWarningAsync(int patientId)
    {
        Patient? patient = await patientRepository.GetByIdAsync(patientId);

        if (patient is not null)
        {
            patient.MedicalHistory = await historyRepository.GetByPatientIdAsync(patientId);
        }

        if (patient?.MedicalHistory?.ChronicConditions is not null
            && patient.MedicalHistory.ChronicConditions.Count != 0)
        {
            return "Patient has underlying conditions that may affect transplant success.";
        }

        return null;
    }

    private async Task<float> CalculatePostMortemScoreAsync(Patient donor, Patient receiver)
    {
        float score = compatibilityService.CalculateScore(donor, receiver);
        DateTime threeMonthsAgo = DateTime.UtcNow.AddMonths(-TimeIntervalMonths);
        int erVisits = await recordRepository.GetERVisitCountAsync(receiver.Id, threeMonthsAgo);
        score += erVisits >= ComparativeERVisits ? MaxScoreModifier : MinScoreModifier;
        return score;
    }

    private static string NormalizeOrganType(string organType)
    {
        string normalized = organType.Trim();
        return normalized switch
        {
            "Lungs" => "Lung",
            _ => normalized,
        };
    }
}
