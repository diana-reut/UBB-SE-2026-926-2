using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System.Data.Common;

namespace HospitalManagement.Repository;


internal class PatientRepository : IPatientRepository
{
    private readonly IDbContext _context;

    public PatientRepository(IDbContext context)
    {
        _context = context;
    }

    public Patient? GetById(int id)
    {
        string query = $"SELECT * FROM Patient WHERE PatientID={id}";
        using var reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return MapToPatient(reader);
        }

        return null;
    }

    private static Patient MapToPatient(DbDataReader reader)
    {
        return new Patient
        {
            Id = reader.GetInt32(reader.GetOrdinal("PatientID")),

            FirstName = reader["FirstName"] as string ?? "",
            LastName = reader["LastName"] as string ?? "",
            Cnp = reader["CNP"] as string ?? "",

            Dob = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),

            Dod = reader.IsDBNull(reader.GetOrdinal("DateOfDeath"))
          ? null
          : reader.GetDateTime(reader.GetOrdinal("DateOfDeath")),

            Sex = Enum.TryParse<Sex>(reader["Sex"]?.ToString(), out var sex) ? sex : Sex.M,

            PhoneNo = reader["Phone"] as string ?? "",
            EmergencyContact = reader["EmergencyContact"] as string ?? "",

            IsArchived = reader.GetBoolean(reader.GetOrdinal("Archived")),
            IsDonor = reader.GetBoolean(reader.GetOrdinal("IsDonor")),
        };
    }

    public List<Patient> GetAll(bool include_archived)
    {
        var patients = new List<Patient>();


        string query = "SELECT * FROM Patient";

        if (!include_archived)
        {
            // SQL Server BIT 0 is false
            query += " WHERE Archived = 0";
        }

        _context.EnsureConnectionOpen();

        using (var reader = _context.ExecuteQuery(query))
        {
            while (reader.Read())
            {
                patients.Add(MapToPatient(reader));
            }
        }

        return patients;
    }

    public List<Patient> GetArchived()
    {
        var archivedPatients = new List<Patient>();

        const string Query = "SELECT * FROM Patient WHERE Archived=1";

        using (var reader = _context.ExecuteQuery(Query))
        {
            while (reader.Read())
            {
                archivedPatients.Add(MapToPatient(reader));
            }
        }

        return archivedPatients;
    }

    public List<Patient> Search(PatientFilter patientFilter)
    {
        ArgumentNullException.ThrowIfNull(patientFilter);

        IEnumerable<Patient> patients = GetAll(true);

        if (!string.IsNullOrWhiteSpace(patientFilter.NamePart))
        {
            patients = patients.Where(p =>
                p.FirstName.Contains(patientFilter.NamePart, StringComparison.OrdinalIgnoreCase)
                    || p.LastName.Contains(patientFilter.NamePart, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(patientFilter.CNP))
        {
            patients = patients.Where(p => p.Cnp.StartsWith(patientFilter.CNP, StringComparison.Ordinal));
        }

        int currentYear = DateTime.Now.Year;
        if (patientFilter.MinAge.HasValue)
        {
            patients = patients.Where(p => currentYear - p.Dob.Year >= patientFilter.MinAge);
        }

        if (patientFilter.MaxAge.HasValue)
        {
            patients = patients.Where(p => currentYear - p.Dob.Year <= patientFilter.MaxAge);
        }

        // MedicalHistory is not loaded by the repository queries, so this branch
        // (p.MedicalHistory is not null) is never true at runtime.
        if (patientFilter.BloodType.HasValue)
        {
            patients = patients.Where(p => p.MedicalHistory is not null && p.MedicalHistory.BloodType == patientFilter.BloodType);
        }

        if (patientFilter.Sex.HasValue)
        {
            patients = patients.Where(p => p.Sex == patientFilter.Sex);
        }

        if (patientFilter.HasChronicCond == true)
        {
            patients = patients.Where(p => p.MedicalHistory?.ChronicConditions.Count != 0);
        }

        return [.. patients];
    }
    protected virtual IEnumerable<Patient> GetPotentialDonors()
    {
        return GetAll(false);
    }

    private List<Patient> GetCompatibleDonors(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge)
    {
        //IEnumerable<Patient> potentialDonors = GetAll(false);
        IEnumerable<Patient> potentialDonors = GetPotentialDonors();

        potentialDonors = potentialDonors.Where(p => p.MedicalHistory is not null);

        int currentYear = DateTime.Now.Year;
        // exclude if not in the age range
        potentialDonors = potentialDonors.Where(p => currentYear - p.Dob.Year >= minAge && currentYear - p.Dob.Year <= maxAge);

        // exclude if the possible donor has chronic condition
        potentialDonors = potentialDonors.Where(p => p.MedicalHistory?.ChronicConditions.Count == 0);

        // exclude if the possible donor has an allergy with severity anaphylactic
        potentialDonors = potentialDonors.Where(p => p.MedicalHistory?.Allergies.Any(a => string.Equals(a.SeverityLevel, "anaphylactic", StringComparison.OrdinalIgnoreCase)) != true);

        // exclude if not a blood match
        potentialDonors = potentialDonors.Where(p => p.MedicalHistory is not null && IsABloodMatch(p.MedicalHistory.BloodType, bloodType));

        // exclude if not a rh match
        potentialDonors = potentialDonors.Where(p => p.MedicalHistory is not null && IsARhMatch(p.MedicalHistory.Rh, rh));

        var donorsScore = new Dictionary<Patient, int>();
        foreach (Patient pd in potentialDonors)
        {
            int score = 0;

            if (pd.MedicalHistory.BloodType == bloodType && pd.MedicalHistory.Rh == rh)
            {
                score += 50;
            }
            else
            {
                score += 25;
            }

            if (pd.Sex == sex)
            {
                score += 20;
            }
            else
            {
                score += 10;
            }

            score += CalculateAgeScore(pd.Dob, dob);

            donorsScore.Add(pd, score);
        }

        var sortedPatients = donorsScore.OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
        return sortedPatients;
    }

    private static bool IsABloodMatch(BloodType? donor, BloodType receiver)
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

    private static bool IsARhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null)
        {
            return false;
        }

        if (donor == Rh.Positive && receiver == Rh.Positive)
        {
            return true;
        }

        if (donor == Rh.Negative)
        {
            return true;
        }

        return false;
    }

    private static int CalculateAgeScore(DateTime donorDob, DateTime receiverDob)
    {
        int currentYear = DateTime.Now.Year;

        int donorAge = currentYear - donorDob.Year;
        int recipientAge = currentYear - receiverDob.Year;
        int ageGap = Math.Abs(donorAge - recipientAge);

        int group = ageGap / 5; // 0 for 0-4, 1 for 5-9, etc.
        int score = 30 - group * 5;
        return Math.Max(score, 0);
    }

    public bool Exists(string cnp)
    {
        string query = $"SELECT * FROM Patient WHERE CNP={cnp}";

        using DbDataReader reader = _context.ExecuteQuery(query);
        return reader.Read();
    }

    public void MarkAsDeceased(int id, DateOnly dod)
    {
        string query = $"UPDATE Patient SET DateOfDeath={dod} WHERE PatientID={id}";
        _ = _context.ExecuteNonQuery(query);
    }

    // public void Add(Patient patientToAdd)
    // {
    //    string query = $"INSERT INTO Patient VALUES({patientToAdd.FirstName}, {patientToAdd.LastName}," +
    //        $"{patientToAdd.Cnp}, {patientToAdd.Dob}, {patientToAdd.Dod}, {patientToAdd.Sex}," +
    //        $"{patientToAdd.PhoneNo}, {patientToAdd.EmergencyContact}, {patientToAdd.IsArchived}, {patientToAdd.IsDonor})";
    //        _context.ExecuteNonQuery(query);
    // }

    public void Add(Patient p)
    {
        // 1. List the columns explicitly (Highly Recommended)
        // 2. Wrap all text/dates in '{value}'
        ArgumentNullException.ThrowIfNull(p);

        string query = "INSERT INTO Patient (FirstName, LastName, Cnp, DateOfBirth, Sex, Phone, EmergencyContact, Archived, IsDonor) "
            + "VALUES ("
            + $"'{p.FirstName}', "
            + $"'{p.LastName}', "
            + $"'{p.Cnp}', "
            + $"'{p.Dob:yyyy-MM-dd}', "
            + $"'{p.Sex}', "
            + $"'{p.PhoneNo}', "
            + $"'{p.EmergencyContact}', "
            + $"{(p.IsArchived ? 1 : 0)}, "
            + $"{(p.IsDonor ? 1 : 0)}); "
            + "SELECT SCOPE_IDENTITY();";

        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read() && int.TryParse(reader[0].ToString(), out int newId))
        {
            p.Id = newId;
        }
    }

    // public void Update(Patient patientToUpdate)
    // {
    //    string query = $"UPDATE Patient SET FirstName={patientToUpdate.FirstName}, LastName={patientToUpdate.LastName}," +
    //        $"Cnp={patientToUpdate.Cnp}, Dob={patientToUpdate.Dob}, Dod={patientToUpdate.Dod}, Sex={patientToUpdate.Sex}," +
    //        $"PhoneNo={patientToUpdate.PhoneNo}, EmergencyContact={patientToUpdate.EmergencyContact}, IsArchived={patientToUpdate.IsArchived}, IsDonor={patientToUpdate.IsDonor}" +
    //        $"WHERE PatientID={patientToUpdate.Id}";
    //        _context.ExecuteNonQuery(query);
    // }

    public void Update(Patient patientToUpdate)
    {
        if (patientToUpdate is null)
        {
            throw new ArgumentNullException(nameof(patientToUpdate), "Patient to update cannot be null.");
        }

        // Handle Nullable Date of Death for SQL string
        string dodValue = patientToUpdate.Dod.HasValue
            ? $"'{patientToUpdate.Dod.Value:yyyy-MM-dd HH:mm:ss}'"
            : "NULL";

        // Format Date of Birth for SQL string
        string dobValue = $"'{patientToUpdate.Dob:yyyy-MM-dd}'";

        // Construct the query using the exact column names from your SSMS screenshot
        string query = $@"UPDATE Patient SET 
        FirstName = '{patientToUpdate.FirstName}', 
        LastName = '{patientToUpdate.LastName}', 
        CNP = '{patientToUpdate.Cnp}', 
        DateOfBirth = {dobValue}, 
        DateOfDeath = {dodValue}, 
        Sex = '{(patientToUpdate.Sex == Sex.M ? "M" : "F")}', 
        Phone = '{patientToUpdate.PhoneNo}', 
        EmergencyContact = '{patientToUpdate.EmergencyContact}', 
        Archived = {(patientToUpdate.IsArchived ? 1 : 0)}, 
        IsDonor = {(patientToUpdate.IsDonor ? 1 : 0)} 
        WHERE PatientID = {patientToUpdate.Id}";

        _ = _context.ExecuteNonQuery(query);
    }

    public void Delete(int id)
    {
        string query = $"DELETE FROM Patient WHERE PatientID={id}";
        _ = _context.ExecuteNonQuery(query);
    }
}
