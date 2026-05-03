using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;

namespace HospitalManagement.Service;

internal class BloodCompatibilityService : IBloodCompatibilityService
{
    private readonly IPatientRepository _patientRepo;
    // 1. Add the History Repository
    private readonly IMedicalHistoryRepository _historyRepo;

    public BloodCompatibilityService(IPatientRepository patientRepo, IMedicalHistoryRepository historyRepo)
    {
        _patientRepo = patientRepo;
        _historyRepo = historyRepo;
    }

    public List<Patient> GetTopCompatibleDonors(int recipientId)
    {
        Patient? recipient = _patientRepo.GetById(recipientId);

        // 2. Actually fetch the Recipient's Medical History from the Database
        if (recipient is not null)
        {
            recipient.MedicalHistory = _historyRepo.GetByPatientId(recipientId);
        }

        if (recipient is null
            || recipient.MedicalHistory is null
            || recipient.MedicalHistory.BloodType is null
            || recipient.MedicalHistory.Rh is null)
        {
            return [];
        }

        List<Patient> allPatients = _patientRepo.GetAll(include_archived: false);
        var rankedDonors = new List<(Patient Donor, int Score)>();

        foreach (Patient donor in allPatients)
        {
            if (donor.Id == recipientId)
            {
                continue;
            }

            if (!donor.IsDonor)
            {
                continue;
            }

            // 3. Actually fetch the Donor's Medical History from the Database
            donor.MedicalHistory = _historyRepo.GetByPatientId(donor.Id);

            // Now their strict rules will actually work!
            if (donor.MedicalHistory is null)
            {
                continue;
            }

            if (donor.MedicalHistory.BloodType is null || donor.MedicalHistory.Rh is null)
            {
                continue;
            }

            if (!IsBloodMatch(donor.MedicalHistory.BloodType, recipient.MedicalHistory.BloodType!.Value))
            {
                continue;
            }

            if (!IsRhMatch(donor.MedicalHistory.Rh, recipient.MedicalHistory.Rh!.Value))
            {
                continue;
            }

            if (donor.MedicalHistory.ChronicConditions?.Count > 0)
            {
                continue;
            }

            if (donor.MedicalHistory.Allergies?.Any(a => a.SeverityLevel.Equals("Anaphylactic", StringComparison.OrdinalIgnoreCase)) == true)
            {
                continue;
            }

            int currentScore = CalculateScore(donor, recipient);
            rankedDonors.Add((donor, currentScore));
        }

        return [.. rankedDonors
            .OrderByDescending(x => x.Score)
            .Select(x => x.Donor)
            .Take(20)];
    }

    public int CalculateScore(Patient donor, Patient recipient)
    {
        int total = 0;

        if (donor is null || recipient is null)
        {
            return 0;
        }

        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
        {
            return 0;
        }

        if (donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
            && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh)
        {
            total += 50;
        }
        else
        {
            total += 25;
        }

        int ageGap = Math.Abs(donor.Dob.Year - recipient.Dob.Year);
        int agePoints = 30 - ageGap / 5 * 5;
        total += Math.Max(0, agePoints);

        total += donor.Sex == recipient.Sex ? 20 : 10;

        return total;
    }

    public bool IsBloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null)
        {
            return false;
        }

        if (donor == BloodType.O)
        {
            return true;
        }

        if (donor == BloodType.A && (receiver == BloodType.A || receiver == BloodType.AB))
        {
            return true;
        }

        if (donor == BloodType.B && (receiver == BloodType.B || receiver == BloodType.AB))
        {
            return true;
        }

        if (donor == BloodType.AB && receiver == BloodType.AB)
        {
            return true;
        }

        return false;
    }

    public bool IsRhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null)
        {
            return false;
        }

        if (receiver == Rh.Negative)
        {
            return donor == Rh.Negative;
        }

        return true;
    }
}
