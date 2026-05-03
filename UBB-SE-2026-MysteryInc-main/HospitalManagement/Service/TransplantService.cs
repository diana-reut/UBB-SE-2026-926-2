using System;
using System.Collections.Generic;
using System.Linq;
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
        _ = _patientRepo.GetById(receiverId) ?? throw new ArgumentException("Receiver not found.");

        var request = new Transplant
        {
            ReceiverId = receiverId,
            DonorId = null,
            OrganType = organType,
            RequestDate = DateTime.Now,
            Status = TransplantStatus.Pending,
            CompatibilityScore = 0,
        };

        _transplantRepo.Add(request);
    }

    public List<Transplant> GetTopMatchesForDonor(int donorId, string organType)
    {
        Patient? donor = _patientRepo.GetById(donorId);
        if (donor?.IsDeceased != true || !donor.IsDonor)
        {
            throw new InvalidOperationException("Donor must be deceased and registered.");
        }

        donor.MedicalHistory = _historyRepo.GetByPatientId(donor.Id);

        List<Transplant> waitlist = _transplantRepo.GetWaitingByOrgan(organType);
        var scoredMatches = new List<Transplant>();

        foreach (Transplant request in waitlist)
        {
            Patient? receiver = _patientRepo.GetById(request.ReceiverId);
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

            if (receiver.MedicalHistory.ChronicConditions is not null && receiver.MedicalHistory.ChronicConditions.Count != 0)
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

    public List<TransplantMatch> GetTopMatchesAsDisplayModels(int donorID, string organType)
    {
        List<Transplant> matches = GetTopMatchesForDonor(donorID, organType);
        var result = new List<TransplantMatch>();

        foreach (Transplant transplant in matches)
        {
            Patient? receiver = _patientRepo.GetById(transplant.ReceiverId);
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

    public void AssignDonor(int transplantId, int donorId, float finalScore)
    {
        _transplantRepo.Update(transplantId, donorId, finalScore);
    }

    public bool IsUrgent(int patientId)
    {
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
        int erVisits = _recordRepo.GetERVisitCount(patientId, threeMonthsAgo);
        return erVisits >= 10;
    }

    public string? GetChronicWarning(int patientId)
    {
        Patient? patient = _patientRepo.GetById(patientId);

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

    private float CalculatePostMortemScore(Patient donor, Patient receiver)
    {
        float score = _compatibilityService.CalculateScore(donor, receiver);
        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-TimeIntervalMonths);
        int erVisits = _recordRepo.GetERVisitCount(receiver.Id, threeMonthsAgo);
        score += erVisits >= ComparativeERVisits ? MaxScoreModifier : MinScoreModifier;
        return score;
    }
}
