using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Repository;
using Common.Data.Entity.Enums;
using Common.Data.Entity;

namespace Common.API.Services;

internal class BillingService : IBillingService
{
    private readonly IMedicalHistoryRepository _historyRepo;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository _prescriptionRepo;
    private readonly ITransplantRepository _transplantRepo;

    public BillingService(IMedicalHistoryRepository historyRepo, IMedicalRecordRepository recordRepo, IPrescriptionRepository prescriptionRepo, ITransplantRepository transplantRepo)
    {
        _historyRepo = historyRepo;
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
        _transplantRepo = transplantRepo;
    }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
    {
        MedicalRecord? record = await _recordRepo.GetByIdAsync(recordId);
        Prescription? prescription = await _prescriptionRepo.GetByRecordIdAsync(recordId);
        List<PrescriptionItem> prescriptionItems = prescription is not null
            ? await _prescriptionRepo.GetItemsAsync(prescription.Id)
            : [];
        MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(patientId);
        List<string> chronicConditions = history is not null
            ? await _historyRepo.GetChronicConditionsAsync(history.Id)
            : [];
        List<(Allergy Allergy, string SeverityLevel)> allergies = history is not null
            ? await _historyRepo.GetAllergiesByHistoryIdAsync(history.Id)
            : [];
        List<Transplant> associatedTransplants = await _transplantRepo.GetByReceiverIdAsync(patientId);

        return CalculateBasePrice(record, history, prescriptionItems, chronicConditions, allergies, associatedTransplants);
    }

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
    {
        return await Task.FromResult(basePrice - basePrice * discount / 100);
    }

    private static decimal CalculateBasePrice(
        MedicalRecord? record,
        MedicalHistory? history,
        List<PrescriptionItem> prescriptionItems,
        List<string> chronicConditions,
        List<(Allergy Allergy, string SeverityLevel)> allergies,
        List<Transplant> associatedTransplants)
    {
        decimal score = 0;

        if (history is null || record is null)
        {
            return score;
        }

        if (record.SourceType == SourceType.ER)
        {
            score += 500;
        }
        else if (record.SourceType == SourceType.App)
        {
            score += 200;
        }

        score += 50 * prescriptionItems.Count;
        score += 100 * chronicConditions.Count;

        foreach ((Allergy Allergy, string SeverityLevel) allergy in allergies)
        {
            if (string.Equals(allergy.SeverityLevel, "mild", StringComparison.OrdinalIgnoreCase) || string.Equals(allergy.SeverityLevel, "moderate", StringComparison.OrdinalIgnoreCase))
            {
                score += 20;
            }
            else if (string.Equals(allergy.SeverityLevel, "severe", StringComparison.OrdinalIgnoreCase) || string.Equals(allergy.SeverityLevel, "anaphylactic", StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
        }

        if (associatedTransplants.Count > 0)
        {
            score += 2000;
        }

        return score;
    }
}
