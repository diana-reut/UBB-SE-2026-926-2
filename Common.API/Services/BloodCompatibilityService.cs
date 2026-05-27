using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;

namespace Common.API.Services;

public class BloodCompatibilityService : IBloodCompatibilityService
{
    private readonly IPatientRepository patientRepo;
    private readonly IMedicalHistoryRepository historyRepo;

    public BloodCompatibilityService(
        IPatientRepository patientRepo,
        IMedicalHistoryRepository historyRepo)
    {
        this.patientRepo = patientRepo;
        this.historyRepo = historyRepo;
    }

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId)
    {
        Patient? recipient = await patientRepo.GetByIdAsync(recipientId);

        if (recipient != null)
        {
            recipient.MedicalHistory =
                await historyRepo.GetByPatientIdAsync(recipientId);
        }

        if (recipient?.MedicalHistory?.BloodType is null
            || recipient.MedicalHistory.Rh is null)
        {
            return new List<Patient>();
        }

        List<Patient> allPatients =
            await patientRepo.GetAllAsync(include_archived: true);

        var rankedDonors = new List<(Patient Donor, int Score)>();

        foreach (var donor in allPatients)
        {
            if (donor.Id == recipientId || donor.IsDeceased)
            {
                continue;
            }

            donor.MedicalHistory =
                await historyRepo.GetByPatientIdAsync(donor.Id);

            if (donor.MedicalHistory?.BloodType is null
                || donor.MedicalHistory.Rh is null)
            {
                continue;
            }

            if (!IsBloodMatch(
                    donor.MedicalHistory.BloodType,
                    recipient.MedicalHistory.BloodType.Value))
            {
                continue;
            }

            if (!IsRhMatch(
                    donor.MedicalHistory.Rh,
                    recipient.MedicalHistory.Rh.Value))
            {
                continue;
            }

            if (donor.MedicalHistory.Allergies?
                .Any(a => a.SeverityLevel.Equals(
                    "Anaphylactic",
                    StringComparison.OrdinalIgnoreCase)) == true)
            {
                continue;
            }

            rankedDonors.Add((donor,
                CalculateScore(donor, recipient)));
        }

        return rankedDonors
            .OrderByDescending(x => x.Score)
            .Select(x => x.Donor)
            .Take(20)
            .ToList();
    }

    public int CalculateScore(Patient donor, Patient recipient)
    {
        int total = 0;

        if (donor.MedicalHistory is null
            || recipient.MedicalHistory is null)
        {
            return 0;
        }

        total += donor.MedicalHistory.BloodType ==
                 recipient.MedicalHistory.BloodType
                 && donor.MedicalHistory.Rh ==
                 recipient.MedicalHistory.Rh
            ? 50
            : 25;

        int ageGap = Math.Abs(donor.Dob.Year - recipient.Dob.Year);
        total += Math.Max(0, 30 - (ageGap / 5 * 5));

        total += donor.Sex == recipient.Sex ? 20 : 10;

        return total;
    }

    public bool IsBloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null)
        {
            return false;
        }

        return donor switch
        {
            BloodType.O => true,
            BloodType.A => receiver is BloodType.A or BloodType.AB,
            BloodType.B => receiver is BloodType.B or BloodType.AB,
            BloodType.AB => receiver == BloodType.AB,
            _ => false
        };
    }

    public bool IsRhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null)
        {
            return false;
        }

        return receiver != Rh.Negative || donor == Rh.Negative;
    }
}
