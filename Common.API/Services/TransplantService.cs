using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Repository;
using Common.Data.Service;

namespace Common.API.Service;

public class TransplantService : ITransplantService
{
    private readonly ITransplantRepository _transplantRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IMedicalRecordRepository _recordRepository;
    private readonly IBloodCompatibilityService _compatibilityService;
    private readonly IMedicalHistoryRepository _historyRepository;

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
        _transplantRepository = transplantRepository;
        _patientRepository = patientRepository;
        _recordRepository = recordRepository;
        _compatibilityService = compatibilityService;
        _historyRepository = historyRepository;
    }

    public Task<Transplant?> GetByIdAsync(int id)
    {
        return _transplantRepository.GetByIdAsync(id);
    }

    public Task<List<Transplant>> GetByReceiverIdAsync(int receiverId)
    {
        return _transplantRepository.GetByReceiverIdAsync(receiverId);
    }

    public Task<List<Transplant>> GetByDonorIdAsync(int donorId)
    {
        return _transplantRepository.GetByDonorIdAsync(donorId);
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
    {
        _ = await _patientRepository.GetByIdAsync(receiverId) ?? throw new ArgumentException("Receiver not found.");

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

        await _transplantRepository.AddAsync(request);
    }

    public Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
    {
        return _transplantRepository.UpdateAsync(transplantId, donorId, finalScore);
    }

    public async Task<List<Transplant>> GetTopMatchesForDonorAsync(int donorId, string organType)
    {
        string normalizedOrganType = NormalizeOrganType(organType);
        Patient? donor = await _patientRepository.GetByIdAsync(donorId);

        if (donor?.IsDeceased != true || !donor.IsDonor)
            throw new InvalidOperationException("Donor must be deceased and registered.");

        donor.MedicalHistory = await _historyRepository.GetByPatientIdAsync(donor.Id);

        List<Transplant> waitlist = await _transplantRepository.GetWaitingByOrganAsync(normalizedOrganType);
        var scoredMatches = new List<Transplant>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = await _patientRepository.GetByIdAsync(request.ReceiverId);
            if (receiver is null)
                continue;

            receiver.MedicalHistory = await _historyRepository.GetByPatientIdAsync(receiver.Id);

            if (receiver.MedicalHistory?.BloodType is null || receiver.MedicalHistory.Rh is null)
                continue;

            if (!_compatibilityService.IsBloodMatch(donor.MedicalHistory?.BloodType, receiver.MedicalHistory.BloodType.Value))
                continue;

            if (!_compatibilityService.IsRhMatch(donor.MedicalHistory?.Rh, receiver.MedicalHistory.Rh.Value))
                continue;

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
            Patient? receiver = await _patientRepository.GetByIdAsync(transplant.ReceiverId);
            MedicalHistory? receiverHistory = receiver is not null ? await _historyRepository.GetByPatientIdAsync(receiver.Id) : null;
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
        int erVisits = await _recordRepository.GetERVisitCountAsync(patientId, threeMonthsAgo);
        return erVisits >= ComparativeERVisits;
    }

    public async Task<string?> GetChronicWarningAsync(int patientId)
    {
        Patient? patient = await _patientRepository.GetByIdAsync(patientId);

        if (patient is not null)
            patient.MedicalHistory = await _historyRepository.GetByPatientIdAsync(patientId);

        if (patient?.MedicalHistory?.ChronicConditions is not null
            && patient.MedicalHistory.ChronicConditions.Count != 0)
        {
            return "Patient has underlying conditions that may affect transplant success.";
        }

        return null;
    }

    private async Task<float> CalculatePostMortemScoreAsync(Patient donor, Patient receiver)
    {
        float score = _compatibilityService.CalculateScore(donor, receiver);
        DateTime threeMonthsAgo = DateTime.UtcNow.AddMonths(-TimeIntervalMonths);
        int erVisits = await _recordRepository.GetERVisitCountAsync(receiver.Id, threeMonthsAgo);
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