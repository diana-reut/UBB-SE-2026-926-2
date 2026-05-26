using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using Common.Data.Entity.DTOs;
using Common.Data.Entity.Enums;
using Common.Data.Entity;
using Common.Data.Integration;
using Common.Data.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Common.API.Services;

public class PatientService : IPatientService
{
    private const int CnpLength = 13;
    private const int CnpDobStartIndex = 1;
    private const int CnpDobLength = 6;
    private const int CnpFirstDigitIndex = 0;
    private const int PhoneMinDigits = 10;
    private const int HighRiskErVisitThreshold = 10;
    private const int HighRiskLookbackMonths = -3;
    private const int MinValidId = 1;

    private readonly IPatientRepository _patientRepo;
    private readonly IMedicalHistoryRepository _historyRepo;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionRepository? _prescriptionRepo;

    public PatientService(
        IPatientRepository patientRepo,
        IMedicalHistoryRepository historyRepo,
        IMedicalRecordRepository recordRepo,
        IPrescriptionRepository? prescriptionRepo = null)
    {
        _patientRepo = patientRepo;
        _historyRepo = historyRepo;
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
    }

    public bool ValidateCNP(string cnp, Sex sex, DateTime dob)
    {
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != CnpLength || !cnp.All(char.IsDigit))
        {
            return false;
        }

        int firstDigit = cnp[CnpFirstDigitIndex] - '0';
        bool isMale = sex == Sex.M;
        bool isFirstDigitOdd = firstDigit % 2 != 0;

        if (isMale != isFirstDigitOdd)
        {
            return false;
        }

        string cnpDobPart = cnp.Substring(CnpDobStartIndex, CnpDobLength);
        string expectedDobPart = dob.ToString("yyMMdd", CultureInfo.InvariantCulture);
        return cnpDobPart == expectedDobPart;
    }

    public async Task<Patient> CreatePatientAsync(Patient data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Patient data cannot be null.");
        }

        if (data.Dob >= DateTime.Today)
        {
            throw new ArgumentException("Validation Error: Birth Date must be in the past.");
        }

        if (!ValidateCNP(data.Cnp, data.Sex, data.Dob))
        {
            throw new ArgumentException("Identity Mismatch: The provided CNP does not align with the selected Sex or Date of Birth.");
        }

        if (await _patientRepo.ExistsAsync(data.Cnp))
        {
            throw new ArgumentException("A patient with this CNP already exists.");
        }

        try
        {
            await _patientRepo.AddAsync(data);
        }
        catch (DbUpdateException ex) when (IsDuplicateCnpException(ex))
        {
            throw new ArgumentException("A patient with this CNP already exists.");
        }

        return data;
    }

    public async Task UpdatePatientAsync(Patient data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Patient data cannot be null.");
        }

        if (!ValidateCNP(data.Cnp, data.Sex, data.Dob))
        {
            throw new ArgumentException("Identity Mismatch: CNP does not align with Sex or DOB.");
        }

        if (string.IsNullOrWhiteSpace(data.PhoneNo) || !Regex.IsMatch(data.PhoneNo, $@"^\+*[\d ]{{{PhoneMinDigits},}}$"))
        {
            throw new ArgumentException("Validation Error: Phone number must be exactly 10 digits and contain no letters.");
        }

        try
        {
            await _patientRepo.UpdateAsync(data);
        }
        catch (DbUpdateException ex) when (IsDuplicateCnpException(ex))
        {
            throw new ArgumentException("A patient with this CNP already exists.");
        }
    }

    public async Task ArchivePatientAsync(Patient patient)
    {
        patient.IsArchived = true;
        await _patientRepo.UpdateAsync(patient);
    }

    public async Task DearchivePatientAsync(int id)
    {
        Patient? patient = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Patient not found.");
        patient.IsArchived = false;
        await _patientRepo.UpdateAsync(patient);
    }

    public async Task ArchiveAsDeceasedAsync(int id, DateTime deathDate)
    {
        if (deathDate > DateTime.Now)
        {
            throw new ArgumentException("Validation Error: Death date cannot be in the future.");
        }

        Patient? patient = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Patient not found.");
        patient.IsArchived = true;
        patient.Dod = deathDate;
        await _patientRepo.UpdateAsync(patient);
    }

    public async Task DeletePatientAsync(int id)
    {
        _ = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Cannot delete: Patient with ID {id} was not found.");
        await _patientRepo.DeleteAsync(id);
    }

    public Task<bool> ExistsAsync(string cnp)
    {
        return _patientRepo.ExistsAsync(cnp);
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _patientRepo.GetByIdAsync(patientId);
    }

    public async Task<Patient> GetPatientDetailsAsync(int id)
    {
        Patient? patient = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

        MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(id);
        if (history is null)
        {
            history = new MedicalHistory { PatientId = id };
        }
        else
        {
            history.ChronicConditions = await _historyRepo.GetChronicConditionsAsync(history.Id);
            history.Allergies = await _historyRepo.GetAllergiesByHistoryIdAsync(history.Id);
        }

        var records = new List<MedicalRecord>();
        if (history.Id >= MinValidId)
        {
            records = [.. (await _recordRepo.GetByHistoryIdAsync(history.Id)).OrderByDescending(r => r.ConsultationDate)];
        }

        var prescriptions = new List<Prescription>();
        prescriptions.AddRange(records.Where(r => r.Prescription is not null).Select(r => r.Prescription!));

        patient.MedicalHistory = history;
        history.MedicalRecords = records;
        prescriptions.ForEach(p => p.MedicalRecord = records.FirstOrDefault(r => r.Id == p.RecordId)!);
        return patient;
    }

    public Task<List<Patient>> SearchPatientsAsync(PatientFilter filter)
    {
        if (filter is not null)
        {
            if (filter.MinAge.HasValue && filter.MinAge < 0)
                throw new ArgumentException("Validation Error: Minimum age cannot be negative.");

            if (filter.MaxAge.HasValue && filter.MaxAge < 0)
                throw new ArgumentException("Validation Error: Maximum age cannot be negative.");

            if (filter.MinAge.HasValue && filter.MaxAge.HasValue && filter.MinAge > filter.MaxAge)
                throw new ArgumentException("Validation Error: Minimum age cannot be greater than maximum age.");

            if (!string.IsNullOrWhiteSpace(filter.CNP) && filter.CNP.Length != CnpLength)
                throw new ArgumentException($"Validation Error: CNP must be exactly {CnpLength} digits for an exact search.");

            if (filter.LastUpdatedFrom.HasValue && filter.LastUpdatedTo.HasValue && filter.LastUpdatedFrom.Value > filter.LastUpdatedTo.Value)
                throw new ArgumentException("Validation Error: 'From' date cannot be after 'To' date.");
        }

        return _patientRepo.SearchAsync(filter!);
    }

    public async Task<bool> IsHighRiskPatientAsync(int patientId)
    {
        DateTime fromDate = DateTime.UtcNow.AddMonths(HighRiskLookbackMonths);
        int erVisitCount = await _recordRepo.GetERVisitCountAsync(patientId, fromDate);
        return erVisitCount > HighRiskErVisitThreshold;
    }

    public async Task CreateMedicalHistoryAsync(int patientId, MedicalHistory history)
    {
        _ = await _patientRepo.GetByIdAsync(patientId) ?? throw new ArgumentException($"Patient with ID {patientId} not found.");

        MedicalHistory? existingHistory = await _historyRepo.GetByPatientIdAsync(patientId);
        if (existingHistory is not null)
            throw new ArgumentException($"Patient {patientId} already has a medical history.");

        if (history is null)
            throw new ArgumentException("Medical history data cannot be null.");

        history.PatientId = patientId;
        int historyId = await _historyRepo.CreateAsync(history);

        if (historyId < MinValidId || history.Allergies is null || history.Allergies.Count == 0)
        {
            return;
        }

        await _historyRepo.SaveAllergiesAsync(historyId, history.Allergies);
    }

    public async Task<int> CreateMedicalRecordAsync(int patientId, MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        _ = await _patientRepo.GetByIdAsync(patientId) ?? throw new KeyNotFoundException($"Patient with ID {patientId} not found.");

        MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(patientId);
        if (history is null)
        {
            throw new InvalidOperationException($"Patient {patientId} does not have a medical history.");
        }

        record.HistoryId = history.Id;
        return await _recordRepo.AddAsync(record);
    }

    public async Task CreatePrescriptionAsync(int recordId, Prescription prescription)
    {
        if (_prescriptionRepo is null)
            throw new InvalidOperationException("PrescriptionRepository is not available.");

        ArgumentNullException.ThrowIfNull(prescription);

        _ = await _recordRepo.GetByIdAsync(recordId) ?? throw new KeyNotFoundException($"Medical record {recordId} not found.");

        prescription.RecordId = recordId;
        await _prescriptionRepo.AddAsync(prescription);
    }

    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId)
    {
        if (patientId < MinValidId)
            throw new KeyNotFoundException("Patient ID is invalid.");

        try
        {
            return await _historyRepo.GetByPatientIdAsync(patientId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching medical history: {ex.Message}");
            return null;
        }
    }

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId)
    {
        try
        {
            return await _recordRepo.GetByHistoryIdAsync(historyId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching medical records: {ex.Message}");
            return [];
        }
    }

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId)
    {
        MedicalRecord record = await _recordRepo.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException($"Medical record {recordId} not found.");

        MedicalHistory? history = await _historyRepo.GetByIdAsync(record.HistoryId);
        if (history?.Patient is null)
        {
            throw new KeyNotFoundException($"Patient for medical record {recordId} not found.");
        }

        Prescription? prescription = _prescriptionRepo is null
            ? null
            : await _prescriptionRepo.GetByRecordIdAsync(recordId);

        List<PrescriptionItem> items = [];
        if (prescription is not null)
        {
            items = await _prescriptionRepo!.GetItemsAsync(prescription.Id);
        }

        return new RecordExportDataDto
        {
            Record = record,
            Patient = history.Patient,
            Prescription = prescription,
            Items = items
        };
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int patientId)
    {
        try
        {
            MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(patientId);
            if (history is null)
                return [];

            List<(Allergy Allergy, string SeverityLevel)> allergyTuples = await _historyRepo.GetAllergiesByHistoryIdAsync(history.Id);
            return allergyTuples.ConvertAll(tuple => $"{tuple.Allergy.AllergyName} - {tuple.SeverityLevel}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching allergies: {ex.Message}");
            return [];
        }
    }

    public Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId)
    {
        if (_prescriptionRepo is null)
            throw new InvalidOperationException("PrescriptionRepository is not available.");

        return _prescriptionRepo.GetByRecordIdAsync(recordId);
    }

    private static bool IsDuplicateCnpException(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627)
            && sqlException.Message.Contains("IX_Patient_CNP", StringComparison.OrdinalIgnoreCase);
    }
}
