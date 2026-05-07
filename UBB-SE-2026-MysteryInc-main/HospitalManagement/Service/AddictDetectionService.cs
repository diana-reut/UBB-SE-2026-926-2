using Common.Data.Entity;
using Common.Data.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;

namespace HospitalManagement.Service;

internal class AddictDetectionService : IAddictDetectionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IMedicalHistoryRepository _medicalHistoryRepository;

    private const string ReportHeader = "==================================================\n           LAW ENFORCEMENT ALERT REPORT           \n==================================================";
    private const string ReportFooter = "--------------------------------------------------\nSUSPICIOUS ACTIVITY: SUSPECTED DRUG SHOPPING BEHAVIOR\nCRITERIA MET: MULTIPLE DOCTORS (>=3) WITHIN 30 DAYS\n--- SUPPORTING EVIDENCE (MEDICAL RECORDS) ---";
    private const string ReportPharmacistFooter = "==================================================\nACTION REQUIRED: AWAITING PHARMACIST CONFIRMATION.";

    public AddictDetectionService(IPrescriptionRepository prescriptionRepository, IMedicalHistoryRepository medicalHistoryRepository)
    {
        _prescriptionRepository = prescriptionRepository ?? throw new ArgumentNullException(nameof(prescriptionRepository));
        _medicalHistoryRepository = medicalHistoryRepository ?? throw new ArgumentNullException(nameof(medicalHistoryRepository));
    }

    public List<Patient> GetAddictCandidates()
    {
        List<Patient> flaggedPatients = _prescriptionRepository.GetAddictCandidatePatients();

        foreach (Patient patient in flaggedPatients)
        {
            patient.MedicalHistory = _medicalHistoryRepository.GetByPatientId(patient.Id);

            if (patient.MedicalHistory is not null)
            {
                patient.MedicalHistory.ChronicConditions = _medicalHistoryRepository.GetChronicConditions(patient.MedicalHistory.Id);
            }

            NormalizeMedicalHistory(patient);
        }

        return flaggedPatients;
    }

    public async Task<List<Patient>> GetAddictCandidatesAsync()
    {
        List<Patient> flaggedPatients = await _prescriptionRepository.GetAddictCandidatePatientsAsync();

        foreach (Patient patient in flaggedPatients)
        {
            patient.MedicalHistory = await _medicalHistoryRepository.GetByPatientIdAsync(patient.Id);

            if (patient.MedicalHistory is not null)
            {
                patient.MedicalHistory.ChronicConditions = await _medicalHistoryRepository.GetChronicConditionsAsync(patient.MedicalHistory.Id);
            }

            NormalizeMedicalHistory(patient);
        }

        return flaggedPatients;
    }

    public string BuildPoliceReport(Patient patient)
    {
        if (patient is null || patient.Id <= 0)
        {
            throw new ArgumentException("Invalid patient data for building a police report.");
        }

        var filter = new PrescriptionFilter
        {
            PatientId = patient.Id,
            DateFrom = DateTime.Today.AddDays(-30),
        };

        List<Prescription> recentPrescriptions = _prescriptionRepository.GetFiltered(filter);
        return BuildPoliceReportText(patient, recentPrescriptions);
    }

    public async Task<string> BuildPoliceReportAsync(Patient patient)
    {
        if (patient is null || patient.Id <= 0)
        {
            throw new ArgumentException("Invalid patient data for building a police report.");
        }

        var filter = new PrescriptionFilter
        {
            PatientId = patient.Id,
            DateFrom = DateTime.Today.AddDays(-30),
        };

        List<Prescription> recentPrescriptions = await _prescriptionRepository.GetFilteredAsync(filter);
        return BuildPoliceReportText(patient, recentPrescriptions);
    }

    public string GetChronicConditions(int patientId)
    {
        if (patientId <= 0)
        {
            throw new ArgumentException("Invalid Patient ID.");
        }

        MedicalHistory? history = _medicalHistoryRepository.GetByPatientId(patientId);
        if (history is null)
        {
            return "None reported.";
        }

        if (history.ChronicConditions is null || history.ChronicConditions.Count == 0)
        {
            history.ChronicConditions = _medicalHistoryRepository.GetChronicConditions(history.Id);
        }

        return history.ChronicConditions is null || history.ChronicConditions.Count == 0
            ? "None reported."
            : string.Join(", ", history.ChronicConditions);
    }

    public async Task<string> GetChronicConditionsAsync(int patientId)
    {
        if (patientId <= 0)
        {
            throw new ArgumentException("Invalid Patient ID.");
        }

        MedicalHistory? history = await _medicalHistoryRepository.GetByPatientIdAsync(patientId);
        if (history is null)
        {
            return "None reported.";
        }

        if (history.ChronicConditions is null || history.ChronicConditions.Count == 0)
        {
            history.ChronicConditions = await _medicalHistoryRepository.GetChronicConditionsAsync(history.Id);
        }

        return history.ChronicConditions is null || history.ChronicConditions.Count == 0
            ? "None reported."
            : string.Join(", ", history.ChronicConditions);
    }

    private static void NormalizeMedicalHistory(Patient patient)
    {
        patient.MedicalHistory ??= new MedicalHistory
        {
            ChronicConditions = ["None reported."],
        };

        if (patient.MedicalHistory.ChronicConditions is null || patient.MedicalHistory.ChronicConditions.Count == 0)
        {
            patient.MedicalHistory.ChronicConditions = ["None reported."];
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
            _ = reportBuilder.AppendLine("No matching records pulled for this timeframe.");
        }
        else
        {
            int evidenceCount = 1;
            foreach (Prescription rx in recentPrescriptions)
            {
                string meds = rx.MedicationList?.Count > 0
                    ? string.Join(", ", rx.MedicationList.Select(m => m.MedName))
                    : "Unknown";

                _ = reportBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{evidenceCount}] Medical Record ID: {rx.RecordId}")
                    .AppendLine(CultureInfo.InvariantCulture, $"    Prescription ID: {rx.Id} | Date: {rx.Date:yyyy-MM-dd}")
                    .AppendLine(CultureInfo.InvariantCulture, $"    Dispensed Drugs: {meds}")
                    .AppendLine("");

                evidenceCount++;
            }
        }

        _ = reportBuilder.AppendLine(ReportPharmacistFooter);
        return reportBuilder.ToString();
    }
}
