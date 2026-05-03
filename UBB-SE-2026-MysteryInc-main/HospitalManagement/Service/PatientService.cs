using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using HospitalManagement.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

        // 2. Optional: You could also check if the CNP already exists in the database here!
        // if (_patientRepo.Exists(data.Cnp)) { throw new Exception("Patient already exists!"); }

        _patientRepo.Add(data);

        return data;
    }

    // <summary>
    // SV2: Updates patient data while locking core identity fields (CNP/DOB)
    // and checking if the record is archived.
    // </summary>
    public void UpdatePatient(Patient data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Patient data cannot be null.");
        }

        Patient? existingPatient = _patientRepo.GetById(data.Id) ?? throw new KeyNotFoundException($"Patient with ID {data.Id} not found.");

        if (existingPatient.Cnp != data.Cnp || existingPatient.Dob.Date != data.Dob.Date)
        {
            throw new InvalidOperationException("ValidationException: Identity cannot be modified. CNP and DOB must match the original record.");
        }

        if (!ValidateCNP(data.Cnp, data.Sex, data.Dob))
        {
            throw new ArgumentException("Identity Mismatch: CNP does not align with Sex or DOB.");
        }

        if (string.IsNullOrWhiteSpace(data.PhoneNo))
        {
            throw new ArgumentException("Validation Error: Phone number must be exactly 10 digits and contain no letters.");
        }

        if (!Regex.IsMatch(data.PhoneNo, @"^\+*[\d ]{10,}$"))
        {
            throw new ArgumentException("Validation Error: Phone number must be exactly 10 digits and contain no letters.");
        }


        // if (string.IsNullOrWhiteSpace(data.EmergencyContact) || !data.EmergencyContact.All(char.IsDigit) || data.EmergencyContact.Length != 10)
        // {
        //    throw new ArgumentException("Validation Error: Emergency contact must contain only numbers.");
        // }

        // 5. Repository Call: Pass the clean object to the repository
        // TODO: Uncomment once the Update method in PatientRepository
        _patientRepo.Update(data);
    }

    // <summary>
    // SV5: Sets the IsArchived bit to true (1) to hide the patient from active views.
    // </summary>
    public void ArchivePatient(int id)
    {
        Patient? patient = _patientRepo.GetById(id) ?? throw new KeyNotFoundException("Patient not found.");

        patient.IsArchived = true;

        // TODO: Uncomment once Repository Update is ready
        _patientRepo.Update(patient);
    }

    // <summary>
    // SV5: Sets the IsArchived bit back to false (0) to restore patient visibility.
    // </summary>
    public void DearchivePatient(int id)
    {
        Patient? patient = _patientRepo.GetById(id) ?? throw new KeyNotFoundException("Patient not found.");

        patient.IsArchived = false;

        // TODO: Uncomment once Repository Update is ready
        _patientRepo.Update(patient);
    }

    // <summary>
    // SV6: Special archive status for deceased patients.
    // Marks them as Archived and records the death date.
    // </summary>
    public void ArchiveAsDeceased(int id, DateTime deathDate)
    {
        if (deathDate > DateTime.Now)
        {
            throw new ArgumentException("Validation Error: Death date cannot be in the future.");
        }

        Patient? patient = _patientRepo.GetById(id) ?? throw new KeyNotFoundException("Patient not found.");

        // Standard archiving
        patient.IsArchived = true;

        // Assuming your Patient entity has a DeathDate property
        patient.Dod = deathDate;

        // TODO: Uncomment once Repository Update is ready
        _patientRepo.Update(patient);
    }

    // <summary>
    // SV7: Validates the filter criteria and calls the repository to search patients.
    // </summary>
    public List<Patient> SearchPatients(PatientFilter filter)
    {
        if (filter is not null)
        {
            // --- 1. Age Validations ---
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

            // --- 2. CNP Validations ---
            // Since the repo looks for an exact match, it must be valid if provided.
            if (!string.IsNullOrWhiteSpace(filter.CNP) && filter.CNP.Length != 13)
            {
                throw new ArgumentException("Validation Error: CNP must be exactly 13 digits for an exact search.");
            }

            // --- 3. Date Validations ---
            if (filter.LastUpdatedFrom.HasValue && filter.LastUpdatedTo.HasValue && filter.LastUpdatedFrom.Value > filter.LastUpdatedTo.Value)
            {
                throw new ArgumentException("Validation Error: 'From' date cannot be after 'To' date.");
            }
        }

        // 4. Pass the clean, validated filter to the Repository!
        return _patientRepo.Search(filter!);
    }

    // <summary>
    // SV3: Initializes the clinical profile for a patient.
    // </summary>
    public void CreateMedicalHistory(int patientId, MedicalHistory history)
    {
        // 1. Validate the patient exists
        _ = _patientRepo.GetById(patientId) ?? throw new ArgumentException($"Patient with ID {patientId} not found.");

        // 2. Prevent duplicate histories
        MedicalHistory? existingHistory = _historyRepo.GetByPatientId(patientId);
        if (existingHistory is not null)
        {
            throw new ArgumentException($"Patient {patientId} already has a medical history.");
        }

        // 3. Link the history to the patient and save
        if (history is null)
        {
            throw new ArgumentException("Medical history data cannot be null.");
        }

        history.PatientId = patientId;
        int historyId = _historyRepo.Create(history);

        if (historyId > 0 && history.Allergies?.Count > 0)
        {
            // 4. Save allergies to PatientAllergies table
            _historyRepo.SaveAllergies(historyId, history.Allergies);
        }
    }

    // <summary>
    // SV4: Fetches the complete patient profile, including linked history and records.
    // </summary>
    public Patient GetPatientDetails(int id)
    {
        // 1. Core Fetch
        Patient? patient = _patientRepo.GetById(id) ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

        // 2. History Link
        MedicalHistory? history = _historyRepo.GetByPatientId(id);
        if (history is null)
        {
            // Initialize an empty one to avoid UI crashes
            history = new MedicalHistory
            {
                PatientId = id,
            };
        }
        else
        {
            history.ChronicConditions = _historyRepo.GetChronicConditions(history.Id);
            history.Allergies = _historyRepo.GetAllergiesByHistoryId(history.Id);
        }

        // 3. Record Timeline
        var records = new List<MedicalRecord>();
        if (history.Id > 0) // Only fetch records if they have a real history saved in the DB
        {
            records = [.. _recordRepo.GetByHistoryId(history.Id).OrderByDescending(r => r.ConsultationDate)];
        }

        // 4. Assemble the final object
        patient.MedicalHistory = history;
        history.MedicalRecords = records;

        return patient;
    }

    // <summary>
    // SV8: Checks if a patient has more than 10 ER visits in the last 3 months.
    // </summary>
    public bool IsHighRiskPatient(int patientId)
    {
        // 1. Calculate the cutoff date (3 months ago)
        DateTime fromDate = DateTime.UtcNow.AddMonths(-3);

        // 2. Fetch the ER visit count from the Record Repository
        int erVisitCount = _recordRepo.GetERVisitCount(patientId, fromDate);

        // 3. Threshold Logic: return true if > 10, otherwise false
        return erVisitCount > 10;
    }

    // <summary>
    // SV9: Permanently removes a patient from the system.
    // </summary>
    public void DeletePatient(int id)
    {
        // 1. Verify the patient exists
        _ = _patientRepo.GetById(id) ?? throw new KeyNotFoundException($"Cannot delete: Patient with ID {id} was not found.");

        // 2. Permanently remove through the repository
        _patientRepo.Delete(id);
    }

    public bool Exists(string cnp)
    {
        return _patientRepo.Exists(cnp);
    }

    // <summary>
    // Get the medical history for a patient
    // </summary>
    public MedicalHistory? GetMedicalHistory(int patientId)
    {
        if (patientId <= 0)
        {
            throw new KeyNotFoundException("Patient ID is invalid.");
        }

        try
        {
            return _historyRepo.GetByPatientId(patientId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching medical history: {ex.Message}");
            return null;
        }
    }

    // <summary>
    // Get all medical records for a patient
    // </summary>
    public List<MedicalRecord> GetMedicalRecords(int historyId)
    {
        try
        {
            return _recordRepo.GetByHistoryId(historyId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching medical records: {ex.Message}");
            return [];
        }
    }

    // <summary>
    // Get all allergies for a patient as formatted strings
    // </summary>
    public List<string> GetPatientAllergies(int patientId)
    {
        try
        {
            MedicalHistory? history = _historyRepo.GetByPatientId(patientId);
            if (history is null)
            {
                return [];
            }

            List<(Allergy Allergy, string SeverityLevel)> allergyTuples = _historyRepo.GetAllergiesByHistoryId(history.Id);

            // Convert tuples to formatted strings: "AllergyName - Severity"
            return allergyTuples.ConvertAll(tuple => $"{tuple.Allergy.AllergyName} - {tuple.SeverityLevel}")
;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching allergies: {ex.Message}");
            return [];
        }
    }

    // <summary>
    // Get prescription by medical record ID
    // </summary>
    public Prescription? GetPrescriptionByRecordId(int recordId)
    {
        if (_prescriptionRepo is null)
        {
            throw new InvalidOperationException("PrescriptionRepository is not available.");
        }

        return _prescriptionRepo.GetByRecordId(recordId);
    }

    public Patient GetById(int patientId)
    {
        return _patientRepo.GetById(patientId) ?? throw new KeyNotFoundException($"Patient with ID {patientId} not found.");
    }
}
