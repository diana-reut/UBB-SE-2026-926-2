using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using Common.Data.Repository;

namespace Common.API.Services;

internal class AddictDetectionService : IAddictDetectionService
{
    private readonly IPrescriptionRepository prescriptionRepository;
    private readonly IMedicalHistoryRepository medicalHistoryRepository;

    private const string ReportHeader = "==================================================\n           LAW ENFORCEMENT ALERT REPORT           \n==================================================";
    private const string ReportFooter = "--------------------------------------------------\nSUSPICIOUS ACTIVITY: SUSPECTED DRUG SHOPPING BEHAVIOR\nCRITERIA MET: MULTIPLE DOCTORS (>=3) WITHIN 30 DAYS\n--- SUPPORTING EVIDENCE (MEDICAL RECORDS) ---";
    private const string ReportPharmacistFooter = "==================================================\nACTION REQUIRED: AWAITING PHARMACIST CONFIRMATION.";
    private const int SuspiciousPrescriptionPeriodDays = 30;
    private const int InitialEvidenceNumber = 1;

    private const string NoConditionsReportedText = "None reported.";
    private const string UnknownMedicationText = "Unknown";
    private const string NoMatchingRecordsText = "No matching records pulled for this timeframe.";
    private const string ReportMedicationSeparator = ", ";

    public AddictDetectionService(IPrescriptionRepository prescriptionRepository, IMedicalHistoryRepository medicalHistoryRepository)
    {
        this.prescriptionRepository = prescriptionRepository ?? throw new ArgumentNullException(nameof(prescriptionRepository));
        this.medicalHistoryRepository = medicalHistoryRepository ?? throw new ArgumentNullException(nameof(medicalHistoryRepository));
    }

    public async Task<List<Patient>> GetAddictCandidatesAsync()
    {
        List<Patient> flaggedPatients = await prescriptionRepository.GetAddictCandidatePatientsAsync();

        List<int> notifiedIds = await prescriptionRepository.GetPoliceNotifiedPatientIdsAsync(
            flaggedPatients.Select(p => p.Id));

        foreach (Patient patient in flaggedPatients)
        {
            patient.IsPoliceNotified = notifiedIds.Contains(patient.Id);
            patient.MedicalHistory = await medicalHistoryRepository.GetByPatientIdAsync(patient.Id);
            if (patient.MedicalHistory is not null)
            {
                patient.MedicalHistory.ChronicConditions = await medicalHistoryRepository.GetChronicConditionsAsync(patient.MedicalHistory.Id);
                patient.MedicalHistory.Patient = null;
                patient.MedicalHistory.PatientAllergies = null;
            }
            NormalizeMedicalHistory(patient);
        }
        return flaggedPatients;
    }

    public async Task MarkPoliceNotifiedAsync(int patientId)
    {
        if (patientId <= 0)
        {
            throw new ArgumentException("Invalid patient ID.");
        }

        await prescriptionRepository.MarkPoliceNotifiedAsync(patientId);
    }

    public async Task<string> BuildPoliceReportAsync(int patientId)
    {
        if (patientId <= 0)
        {
            throw new ArgumentException("Invalid patient data for building a police report.");
        }

        var filter = new PrescriptionFilter
        {
            PatientId = patientId,
            DateFrom = DateTime.Today.AddDays(-SuspiciousPrescriptionPeriodDays),
        };

        List<Prescription> recentPrescriptions = await prescriptionRepository.GetFilteredAsync(filter);
        Patient patient = recentPrescriptions.FirstOrDefault()?.MedicalRecord?.History?.Patient
            ?? throw new ArgumentException("Patient not found.");

        return BuildPoliceReportText(patient, recentPrescriptions);
    }

    public async Task<string> GetChronicConditionsAsync(int patientId)
    {
        if (patientId <= 0)
        {
            throw new ArgumentException("Invalid Patient ID.");
        }

        MedicalHistory? history = await medicalHistoryRepository.GetByPatientIdAsync(patientId);
        if (history is null)
        {
            return NoConditionsReportedText;
        }

        if (history.ChronicConditions is null || history.ChronicConditions.Count == 0)
        {
            history.ChronicConditions = await medicalHistoryRepository.GetChronicConditionsAsync(history.Id);
        }

        return history.ChronicConditions is null || history.ChronicConditions.Count == 0
            ? NoConditionsReportedText
            : string.Join(ReportMedicationSeparator, history.ChronicConditions);
    }

    private static void NormalizeMedicalHistory(Patient patient)
    {
        patient.MedicalHistory ??= new MedicalHistory
        {
            ChronicConditions = new List<string> { NoConditionsReportedText },
        };

        if (patient.MedicalHistory.ChronicConditions is null || patient.MedicalHistory.ChronicConditions.Count == 0)
        {
            patient.MedicalHistory.ChronicConditions = new List<string> { NoConditionsReportedText };
        }
    }

    private static string BuildPoliceReportText(Patient patient, List<Prescription> recentPrescriptions)
    {
        var reportBuilder = new System.Text.StringBuilder();

        _ = reportBuilder.AppendLine(ReportHeader)
            .AppendLine(CultureInfo.InvariantCulture, $"DATE GENERATED: {DateTime.Now:yyyy-MM-dd HH:mm}")
            .AppendLine(CultureInfo.InvariantCulture, $"SUBJECT: {patient.FirstName} {patient.LastName} (CNP: {patient.Cnp})")
            .AppendLine(CultureInfo.InvariantCulture, $"CONTACT: {patient.PhoneNo}")
            .AppendLine(ReportFooter);

        if (recentPrescriptions.Count == 0)
        {
            _ = reportBuilder.AppendLine(NoMatchingRecordsText);
        }
        else
        {
            int evidenceCount = InitialEvidenceNumber;
            foreach (Prescription rx in recentPrescriptions)
            {
                string meds = rx.MedicationList?.Count > 0
                    ? string.Join(ReportMedicationSeparator, rx.MedicationList.Select(m => m.MedName))
                    : UnknownMedicationText;

                _ = reportBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{evidenceCount}] Medical Record ID: {rx.RecordId}")
                    .AppendLine(CultureInfo.InvariantCulture, $"    Prescription ID: {rx.Id} | Date: {rx.Date:yyyy-MM-dd}")
                    .AppendLine(CultureInfo.InvariantCulture, $"    Dispensed Drugs: {meds}")
                    .AppendLine(string.Empty);

                evidenceCount++;
            }
        }

        _ = reportBuilder.AppendLine(ReportPharmacistFooter);
        return reportBuilder.ToString();
    }
}
