using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Repository;
using Common.Data.Entity.Enums;
using Common.Data.Entity;

namespace Common.API.Services;

internal class BillingService : IBillingService
{
    private readonly IMedicalHistoryRepository historyRepo;
    private readonly IMedicalRecordRepository recordRepo;
    private readonly IPrescriptionRepository prescriptionRepo;
    private readonly ITransplantRepository transplantRepo;
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
        this.historyRepo = historyRepo;
        this.recordRepo = recordRepo;
        this.prescriptionRepo = prescriptionRepo;
        this.transplantRepo = transplantRepo;
    }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
    {
        MedicalRecord? record = await recordRepo.GetByIdAsync(recordId);
        Prescription? prescription = await prescriptionRepo.GetByRecordIdAsync(recordId);
        List<PrescriptionItem> prescriptionItems = prescription is not null
            ? await prescriptionRepo.GetItemsAsync(prescription.Id)
            : new List<PrescriptionItem>();
        MedicalHistory? history = await historyRepo.GetByPatientIdAsync(patientId);
        List<string> chronicConditions = history is not null
            ? await historyRepo.GetChronicConditionsAsync(history.Id)
            : new List<string>();
        List<(Allergy Allergy, string SeverityLevel)> allergies = history is not null
            ? await historyRepo.GetAllergiesByHistoryIdAsync(history.Id)
            : new List<(Allergy Allergy, string SeverityLevel)>();
        List<Transplant> associatedTransplants = await transplantRepo.GetByReceiverIdAsync(patientId);

        return CalculateBasePrice(record, history, prescriptionItems, chronicConditions, allergies, associatedTransplants);
    }

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
    {
        return await Task.FromResult(basePrice - (basePrice * discount / PercentageDivisor));
    }

    public async Task<decimal> PersistDiscountAsync(int recordId, decimal basePrice, int discount)
    {
        decimal finalPrice = await ApplyDiscountAsync(basePrice, discount);

        MedicalRecord? record = await recordRepo.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException($"Medical record with ID {recordId} not found.");

        record.BasePrice = basePrice;
        record.DiscountApplied = discount;
        record.FinalPrice = finalPrice;

        await recordRepo.UpdateAsync(record);

        return finalPrice;
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
