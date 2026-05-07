using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal class PatientService : IPatientService
{
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
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
        {
            return false;
        }

        int firstDigit = cnp[0] - '0';
        bool isMale = sex == Sex.M;
        bool isFirstDigitOdd = firstDigit % 2 != 0;

        if (isMale != isFirstDigitOdd)
        {
            return false;
        }

        string cnpDobPart = cnp.Substring(1, 6);
        string expectedDobPart = dob.ToString("yyMMdd", CultureInfo.InvariantCulture);
        return cnpDobPart == expectedDobPart;
    }

    public Patient CreatePatient(Patient data)
    {
        return CreatePatientAsync(data).GetAwaiter().GetResult();
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

        bool isValid = ValidateCNP(data.Cnp, data.Sex, data.Dob);
        if (!isValid)
        {
            throw new ArgumentException("Identity Mismatch: The provided CNP does not align with the selected Sex or Date of Birth.");
        }

        await _patientRepo.AddAsync(data);
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

        if (string.IsNullOrWhiteSpace(data.PhoneNo) || !Regex.IsMatch(data.PhoneNo, @"^\+*[\d ]{10,}$"))
        {
            throw new ArgumentException("Validation Error: Phone number must be exactly 10 digits and contain no letters.");
        }

        await _patientRepo.UpdateAsync(data);
    }

    public void ArchivePatient(int id)
    {
       // ArchivePatientAsync(id).GetAwaiter().GetResult();
    }

    public async Task ArchivePatientAsync(Patient patient)
    {
        patient.IsArchived = true;
        await _patientRepo.UpdateAsync(patient);
    }

    public void DearchivePatient(int id)
    {
        DearchivePatientAsync(id).GetAwaiter().GetResult();
    }

    public async Task DearchivePatientAsync(int id)
    {
        Patient? patient = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Patient not found.");
        patient.IsArchived = false;
        await _patientRepo.UpdateAsync(patient);
    }

    public void ArchiveAsDeceased(int id, DateTime deathDate)
    {
        ArchiveAsDeceasedAsync(id, deathDate).GetAwaiter().GetResult();
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

    public List<Patient> SearchPatients(PatientFilter filter)
    {
        return SearchPatientsAsync(filter).GetAwaiter().GetResult();
    }

    public Task<List<Patient>> SearchPatientsAsync(PatientFilter filter)
    {
        if (filter is not null)
        {
            if (filter.MinAge.HasValue && filter.MinAge < 0)
            {
                throw new ArgumentException("Validation Error: Minimum age cannot be negative.");
            }

            if (filter.MaxAge.HasValue && filter.MaxAge < 0)
            {
                throw new ArgumentException("Validation Error: Maximum age cannot be negative.");
            }

            if (filter.MinAge.HasValue && filter.MaxAge.HasValue && filter.MinAge > filter.MaxAge)
            {
                throw new ArgumentException("Validation Error: Minimum age cannot be greater than maximum age.");
            }

            if (!string.IsNullOrWhiteSpace(filter.CNP) && filter.CNP.Length != 13)
            {
                throw new ArgumentException("Validation Error: CNP must be exactly 13 digits for an exact search.");
            }

            if (filter.LastUpdatedFrom.HasValue && filter.LastUpdatedTo.HasValue && filter.LastUpdatedFrom.Value > filter.LastUpdatedTo.Value)
            {
                throw new ArgumentException("Validation Error: 'From' date cannot be after 'To' date.");
            }
        }

        return _patientRepo.SearchAsync(filter!);
    }

    public void CreateMedicalHistory(int patientId, MedicalHistory history)
    {
        CreateMedicalHistoryAsync(patientId, history).GetAwaiter().GetResult();
    }

    public async Task CreateMedicalHistoryAsync(int patientId, MedicalHistory history)
    {
        _ = await _patientRepo.GetByIdAsync(patientId) ?? throw new ArgumentException($"Patient with ID {patientId} not found.");

        MedicalHistory? existingHistory = await _historyRepo.GetByPatientIdAsync(patientId);
        if (existingHistory is not null)
        {
            throw new ArgumentException($"Patient {patientId} already has a medical history.");
        }

        if (history is null)
        {
            throw new ArgumentException("Medical history data cannot be null.");
        }

        history.PatientId = patientId;
        int historyId = await _historyRepo.CreateAsync(history);

        if (historyId > 0 && history.Allergies?.Count > 0)
        {
            await _historyRepo.SaveAllergiesAsync(historyId, history.Allergies);
        }
    }

    public Patient GetPatientDetails(int id)
    {
        return GetPatientDetailsAsync(id).GetAwaiter().GetResult();
    }

    public async Task<Patient> GetPatientDetailsAsync(int id)
    {
        Patient? patient = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

        MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(id);
        if (history is null)
        {
            history = new MedicalHistory
            {
                PatientId = id,
            };
        }
        else
        {
            history.ChronicConditions = await _historyRepo.GetChronicConditionsAsync(history.Id);
            history.Allergies = await _historyRepo.GetAllergiesByHistoryIdAsync(history.Id);
        }

        var records = new List<MedicalRecord>();
        if (history.Id > 0)
        {
            records = [.. (await _recordRepo.GetByHistoryIdAsync(history.Id)).OrderByDescending(r => r.ConsultationDate)];
        }

        patient.MedicalHistory = history;
        history.MedicalRecords = records;
        return patient;
    }

    public bool IsHighRiskPatient(int patientId)
    {
        return IsHighRiskPatientAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task<bool> IsHighRiskPatientAsync(int patientId)
    {
        DateTime fromDate = DateTime.UtcNow.AddMonths(-3);
        int erVisitCount = await _recordRepo.GetERVisitCountAsync(patientId, fromDate);
        return erVisitCount > 10;
    }

    public void DeletePatient(int id)
    {
        DeletePatientAsync(id).GetAwaiter().GetResult();
    }

    public async Task DeletePatientAsync(int id)
    {
        _ = await _patientRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Cannot delete: Patient with ID {id} was not found.");
        await _patientRepo.DeleteAsync(id);
    }

    public bool Exists(string cnp)
    {
        return ExistsAsync(cnp).GetAwaiter().GetResult();
    }

    public Task<bool> ExistsAsync(string cnp)
    {
        return _patientRepo.ExistsAsync(cnp);
    }

    public MedicalHistory? GetMedicalHistory(int patientId)
    {
        return GetMedicalHistoryAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId)
    {
        if (patientId <= 0)
        {
            throw new KeyNotFoundException("Patient ID is invalid.");
        }

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

    public List<MedicalRecord> GetMedicalRecords(int historyId)
    {
        return GetMedicalRecordsAsync(historyId).GetAwaiter().GetResult();
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

    public List<string> GetPatientAllergies(int patientId)
    {
        return GetPatientAllergiesAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task<List<string>> GetPatientAllergiesAsync(int patientId)
    {
        try
        {
            MedicalHistory? history = await _historyRepo.GetByPatientIdAsync(patientId);
            if (history is null)
            {
                return [];
            }

            List<(Allergy Allergy, string SeverityLevel)> allergyTuples = await _historyRepo.GetAllergiesByHistoryIdAsync(history.Id);
            return allergyTuples.ConvertAll(tuple => $"{tuple.Allergy.AllergyName} - {tuple.SeverityLevel}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching allergies: {ex.Message}");
            return [];
        }
    }

    public Prescription? GetPrescriptionByRecordId(int recordId)
    {
        return GetPrescriptionByRecordIdAsync(recordId).GetAwaiter().GetResult();
    }

    public Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId)
    {
        if (_prescriptionRepo is null)
        {
            throw new InvalidOperationException("PrescriptionRepository is not available.");
        }

        return _prescriptionRepo.GetByRecordIdAsync(recordId);
    }

    public Patient? GetById(int patientId)
    {
        return GetByIdAsync(patientId).GetAwaiter().GetResult();
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _patientRepo.GetByIdAsync(patientId);
    }
}
