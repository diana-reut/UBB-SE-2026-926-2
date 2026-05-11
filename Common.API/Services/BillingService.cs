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
    private const int PercentageDivisor = 100;
    private const decimal EmergencyRoomBasePrice = 500;
    private const decimal AppointmentBasePrice = 200;
    private const decimal PrescriptionItemPrice = 50;
    private const decimal ChronicConditionPrice = 100;
    private const decimal MildOrModerateAllergyPrice = 20;
    private const decimal SevereAllergyPrice = 100;
    private const decimal TransplantAdditionalPrice = 2000;


    private const string MildSeverity = "mild";
    private const string ModerateSeverity = "moderate";
    private const string SevereSeverity = "severe";
    private const string AnaphylacticSeverity = "anaphylactic";

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
        return await Task.FromResult(basePrice - basePrice * discount / PercentageDivisor);
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
            score += EmergencyRoomBasePrice;
        }
        else if (record.SourceType == SourceType.App)
        {
            score += AppointmentBasePrice;
        }

        score += PrescriptionItemPrice * prescriptionItems.Count;
        score += ChronicConditionPrice * chronicConditions.Count;

        foreach ((Allergy Allergy, string SeverityLevel) allergy in allergies)
        {
            if (string.Equals(allergy.SeverityLevel, MildSeverity, StringComparison.OrdinalIgnoreCase) || string.Equals(allergy.SeverityLevel, ModerateSeverity, StringComparison.OrdinalIgnoreCase))
            {
                score += MildOrModerateAllergyPrice;
            }
            else if (string.Equals(allergy.SeverityLevel, SevereSeverity, StringComparison.OrdinalIgnoreCase) || string.Equals(allergy.SeverityLevel, AnaphylacticSeverity, StringComparison.OrdinalIgnoreCase))
            {
                score += SevereAllergyPrice;
            }
        }

        if (associatedTransplants.Count > 0)
        {
            score += TransplantAdditionalPrice;
        }

        return score;
    }
}
