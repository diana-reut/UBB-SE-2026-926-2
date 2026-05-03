using System;
using System.Collections.Generic;
using HospitalManagement.Repository;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Entity;

namespace HospitalManagement.Service;

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

    public decimal ComputeBasePrice(int patientId, int recordId)
    {
        decimal score = 0;
        MedicalRecord? record = _recordRepo.GetById(recordId);
        Prescription? prescription = _prescriptionRepo.GetByRecordId(recordId);
        List<PrescriptionItem> prescriptionItems;
        if (prescription is not null)
        {
            prescriptionItems = _prescriptionRepo.GetItems(prescription.Id);
        }
        else
        {
            prescriptionItems = [];
        }

        MedicalHistory? history = _historyRepo.GetByPatientId(patientId);

        if (history is null || record is null)
        {
            return score;
        }

        List<string> chronicConditions = _historyRepo.GetChronicConditions(history.Id);
        List<(Allergy Allergy, string SeverityLevel)> allergies = _historyRepo.GetAllergiesByHistoryId(history.Id);
        List<Transplant> associatedTransplants = _transplantRepo.GetByReceiverId(patientId);

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

    public decimal ApplyDiscount(decimal basePrice, int discount)
    {
        return basePrice - basePrice * discount / 100;
    }
}
