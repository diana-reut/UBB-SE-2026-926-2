using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;

namespace HospitalManagement.Service;

internal class TransplantService : ITransplantService
{
    private readonly ITransplantRepository _transplantRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IBloodCompatibilityService _compatibilityService;
    private readonly IMedicalHistoryRepository _historyRepo;

    private const int MaxScoreModifier = 20;
    private const int MinScoreModifier = 5;
    private const int ComparativeERVisits = 10;
    private const int TimeIntervalMonths = 3;

    public TransplantService(
        ITransplantRepository transplantRepo,
        IPatientRepository patientRepo,
        IMedicalRecordRepository recordRepo,
        IBloodCompatibilityService compatibilityService,
        IMedicalHistoryRepository historyRepo)
    {
        _transplantRepo = transplantRepo;
        _patientRepo = patientRepo;
        _recordRepo = recordRepo;
        _compatibilityService = compatibilityService;
        _historyRepo = historyRepo;
    }

    public void CreateWaitlistRequest(int receiverId, string organType)
    {
        CreateWaitlistRequestAsync(receiverId, organType).GetAwaiter().GetResult();
    }

    public async Task CreateWaitlistRequestAsync(int receiverId, string organType)
    {
        _ = await _patientRepo.GetByIdAsync(receiverId) ?? throw new ArgumentException("Receiver not found.");
        string normalizedOrganType = NormalizeOrganType(organType);

        var request = new Transplant
        {
            ReceiverId = receiverId,
            DonorId = null,
            OrganType = normalizedOrganType,
            RequestDate = DateTime.Now,
            Status = TransplantStatus.Pending,
            CompatibilityScore = 0,
        };

        await _transplantRepo.AddAsync(request);
    }

    public List<Transplant> GetTopMatchesForDonor(int donorId, string organType)
    {
        string normalizedOrganType = NormalizeOrganType(organType);
        Patient? donor = _patientRepo.GetByIdAsync(donorId).GetAwaiter().GetResult();
        if (donor?.IsDeceased != true || !donor.IsDonor)
        {
            throw new InvalidOperationException("Donor must be deceased and registered.");
        }

        donor.MedicalHistory = _historyRepo.GetByPatientId(donor.Id);

        List<Transplant> waitlist = _transplantRepo.GetWaitingByOrgan(normalizedOrganType);
        var scoredMatches = new List<Transplant>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = _patientRepo.GetByIdAsync(request.ReceiverId).GetAwaiter().GetResult();
            if (receiver is null)
            {
                continue;
            }

            receiver.MedicalHistory = _historyRepo.GetByPatientId(receiver.Id);

            if (receiver.MedicalHistory?.BloodType is null || receiver.MedicalHistory.Rh is null)
            {
                continue;
            }

            if (!_compatibilityService.IsBloodMatch(donor.MedicalHistory?.BloodType, receiver.MedicalHistory.BloodType.Value))
            {
                continue;
            }

            if (!_compatibilityService.IsRhMatch(donor.MedicalHistory?.Rh, receiver.MedicalHistory.Rh.Value))
            {
                continue;
            }

            request.CompatibilityScore = CalculatePostMortemScore(donor, receiver);
            scoredMatches.Add(request);
        }

        return [.. scoredMatches
            .OrderByDescending(t => t.CompatibilityScore)
            .ThenBy(t => t.RequestDate)
            .Take(5)];
    }

    public async Task<List<Transplant>> GetTopMatchesForDonorAsync(int donorId, string organType)
    {
        string normalizedOrganType = NormalizeOrganType(organType);
        Patient? donor = await _patientRepo.GetByIdAsync(donorId);
        if (donor?.IsDeceased != true || !donor.IsDonor)
        {
            throw new InvalidOperationException("Donor must be deceased and registered.");
        }

        donor.MedicalHistory = await _historyRepo.GetByPatientIdAsync(donor.Id);

        List<Transplant> waitlist = await _transplantRepo.GetWaitingByOrganAsync(normalizedOrganType);
        var scoredMatches = new List<Transplant>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = await _patientRepo.GetByIdAsync(request.ReceiverId);
            if (receiver is null)
            {
                continue;
            }

            receiver.MedicalHistory = await _historyRepo.GetByPatientIdAsync(receiver.Id);

            if (receiver.MedicalHistory?.BloodType is null || receiver.MedicalHistory.Rh is null)
            {
                continue;
            }

            if (!_compatibilityService.IsBloodMatch(donor.MedicalHistory?.BloodType, receiver.MedicalHistory.BloodType.Value))
            {
                continue;
            }

            if (!_compatibilityService.IsRhMatch(donor.MedicalHistory?.Rh, receiver.MedicalHistory.Rh.Value))
            {
                continue;
            }

            request.CompatibilityScore = await CalculatePostMortemScoreAsync(donor, receiver);
            scoredMatches.Add(request);
        }

        return [.. scoredMatches
            .OrderByDescending(t => t.CompatibilityScore)
            .ThenBy(t => t.RequestDate)
            .Take(5)];
    }

    public List<TransplantMatch> GetTopMatchesAsDisplayModels(int donorID, string organType)
    {
        List<Transplant> matches = GetTopMatchesForDonor(donorID, organType);
        var result = new List<TransplantMatch>();

        foreach (Transplant transplant in matches)
        {
            Patient? receiver = _patientRepo.GetByIdAsync(transplant.ReceiverId).GetAwaiter().GetResult();
            MedicalHistory? receiverHistory = receiver is not null ? _historyRepo.GetByPatientId(receiver.Id) : null;
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
                WaitingDays = (DateTime.Now - transplant.RequestDate).Days,
            });
        }

        return result;
    }

    public async Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorID, string organType)
    {
        List<Transplant> matches = await GetTopMatchesForDonorAsync(donorID, organType);
        var result = new List<TransplantMatch>();

        foreach (Transplant transplant in matches)
        {
            Patient? receiver = await _patientRepo.GetByIdAsync(transplant.ReceiverId);
            MedicalHistory? receiverHistory = receiver is not null ? await _historyRepo.GetByPatientIdAsync(receiver.Id) : null;
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
                WaitingDays = (DateTime.Now - transplant.RequestDate).Days,
            });
        }

        return result;
    }

    public void AssignDonor(int transplantId, int donorId, float finalScore)
    {
        _transplantRepo.Update(transplantId, donorId, finalScore);
    }

    public Task AssignDonorAsync(int transplantId, int donorId, float finalScore)
    {
        return _transplantRepo.UpdateAsync(transplantId, donorId, finalScore);
    }

    public bool IsUrgent(int patientId)
    {
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
        int erVisits = _recordRepo.GetERVisitCount(patientId, threeMonthsAgo);
        return erVisits >= 10;
    }

    public async Task<bool> IsUrgentAsync(int patientId)
    {
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
        int erVisits = await _recordRepo.GetERVisitCountAsync(patientId, threeMonthsAgo);
        return erVisits >= 10;
    }

    public string? GetChronicWarning(int patientId)
    {
        Patient? patient = _patientRepo.GetByIdAsync(patientId).GetAwaiter().GetResult();

        if (patient is not null)
        {
            patient.MedicalHistory = _historyRepo.GetByPatientId(patientId);
        }

        if (patient?.MedicalHistory?.ChronicConditions is not null
            && patient.MedicalHistory.ChronicConditions.Count != 0)
        {
            return "Patient has underlying conditions that may affect transplant success.";
        }

        return null;
    }

    public async Task<string?> GetChronicWarningAsync(int patientId)
    {
        Patient? patient = await _patientRepo.GetByIdAsync(patientId);

        if (patient is not null)
        {
            patient.MedicalHistory = await _historyRepo.GetByPatientIdAsync(patientId);
        }

        if (patient?.MedicalHistory?.ChronicConditions is not null
            && patient.MedicalHistory.ChronicConditions.Count != 0)
        {
            return "Patient has underlying conditions that may affect transplant success.";
        }

        return null;
    }

    private float CalculatePostMortemScore(Patient donor, Patient receiver)
    {
        float score = _compatibilityService.CalculateScore(donor, receiver);
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-TimeIntervalMonths);
        int erVisits = _recordRepo.GetERVisitCount(receiver.Id, threeMonthsAgo);
        score += erVisits >= ComparativeERVisits ? MaxScoreModifier : MinScoreModifier;
        return score;
    }

    private async Task<float> CalculatePostMortemScoreAsync(Patient donor, Patient receiver)
    {
        float score = _compatibilityService.CalculateScore(donor, receiver);
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-TimeIntervalMonths);
        int erVisits = await _recordRepo.GetERVisitCountAsync(receiver.Id, threeMonthsAgo);
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
