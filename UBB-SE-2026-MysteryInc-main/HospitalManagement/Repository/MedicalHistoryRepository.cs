using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;

namespace HospitalManagement.Repository;

internal class MedicalHistoryRepository : IMedicalHistoryRepository
{
    private readonly IDbContext _context; // RP 12

    public MedicalHistoryRepository(IDbContext context)
    {
        _context = context;
    }

    public MedicalHistory? GetByPatientId(int patientId) // RP 13
    {
        string query = $"SELECT * FROM MedicalHistory WHERE PatientID={patientId}";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return MapToMedicalHistory(reader);
        }

        return null;
    }

    public MedicalHistory? GetById(int historyId) // RP 13
    {
        string query = $"SELECT * FROM MedicalHistory WHERE HistoryID={historyId}";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return MapToMedicalHistory(reader);
        }

        return null;
    }

    public int Create(MedicalHistory history) // RP 14
    {
        // TODO: check if this will return empty or something
        ArgumentNullException.ThrowIfNull(history);

        // Format ChronicConditions as comma-separated string
        string? chronicConditionsStr = history.ChronicConditions.Count > 0
            ? string.Join(", ", history.ChronicConditions)
            : null;

        string query = "INSERT INTO MedicalHistory (PatientID, BloodType, Rh, ChronicConditions) "
            + $"VALUES ({history.PatientId}, "
            + $"{(history.BloodType.HasValue ? $"'{history.BloodType}'" : "NULL")}, "
            + $"{(history.Rh.HasValue ? $"'{history.Rh}'" : "NULL")}, "
            + $"{(string.IsNullOrEmpty(chronicConditionsStr) ? "NULL" : $"'{chronicConditionsStr.Replace("'", "''", StringComparison.Ordinal)}'")});"
            + "SELECT SCOPE_IDENTITY();";

        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read() && int.TryParse(reader[0].ToString(), out int historyId))
        {
            return historyId;
        }

        return -1; // Error case
    }

    public void SaveAllergies(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies)
    {
        if (allergies is null || allergies.Count == 0)
        {
            return;
        }

        foreach ((Allergy? allergy, string? severity) in allergies)
        {
            string query = "INSERT INTO PatientAllergies (AllergyID, HistoryID, SeverityLevel) "
                + $"VALUES ({allergy.AllergyId}, {historyId}, '{severity}')";
            _ = _context.ExecuteNonQuery(query);
        }
    }

    public void Update(MedicalHistory history) // RP14
    {
        ArgumentNullException.ThrowIfNull(history);

        // Verify patientId matches what's in DB for this history record
        string checkQuery = $"SELECT PatientID FROM MedicalHistory WHERE HistoryID={history.Id}";
        using (DbDataReader reader = _context.ExecuteQuery(checkQuery))
        {
            if (!reader.Read())
            {
                throw new KeyNotFoundException($"MedicalHistory with ID={history.Id} not found.");
            }

            int dbPatientId = Convert.ToInt32(reader["PatientID"], CultureInfo.InvariantCulture);
            if (dbPatientId != history.PatientId)
            {
                throw new InvalidOperationException("PatientID mismatch - cannot reassign history to a different patient.");
            }
        }

        string query = "UPDATE MedicalHistory SET "
            + $"BloodType = {(history.BloodType.HasValue ? $"'{history.BloodType}'" : "NULL")}, "
            + $"Rh = {(history.Rh.HasValue ? $"'{history.Rh}'" : "NULL")} "
            + $"WHERE HistoryID = {history.Id}";
        _ = _context.ExecuteNonQuery(query);
    }

    private static MedicalHistory MapToMedicalHistory(DbDataReader reader) // RP 15
    {
        return new MedicalHistory
        {
            Id = Convert.ToInt32(reader["HistoryID"], CultureInfo.InvariantCulture),
            PatientId = Convert.ToInt32(reader["PatientID"], CultureInfo.InvariantCulture),
            BloodType = reader["BloodType"] == DBNull.Value
                        ? null
                        : Enum.Parse<BloodType>(reader["BloodType"].ToString()!),
            Rh = reader["Rh"] == DBNull.Value
                 ? null
                 : Enum.Parse<Rh>(reader["Rh"].ToString()!),
            // ChronicConditions, MedicalRecords, Allergies are loaded separately
            // via their own repositories/queries
        };
    }



    // things from entity put here

    public List<string> GetChronicConditions(int historyId) // part of getRecords() scope
    {
        // ChronicConditions is stored as a single VARCHAR(2000) in DB5
        // so we parse it back into a list here
        string query = $"SELECT ChronicConditions FROM MedicalHistory WHERE HistoryID={historyId}";
        using (DbDataReader reader = _context.ExecuteQuery(query))
        {
            if (reader.Read() && reader["ChronicConditions"] != DBNull.Value)
            {
                string raw = reader["ChronicConditions"].ToString()!;
                return [.. raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            }
        }

        return [];
    }

    public List<(Allergy Allergy, string SeverityLevel)> GetAllergiesByHistoryId(int historyId) // getAllergies()
    {
        var result = new List<(Allergy, string)>();
        string query = "SELECT a.AllergyID, a.AllergyName, a.AllergyType, a.AllergyCategory, pa.SeverityLevel "
            + "FROM PatientAllergies pa "
            + "JOIN Allergy a ON pa.AllergyID = a.AllergyID "
            + $"WHERE pa.HistoryID={historyId}";
        using (DbDataReader reader = _context.ExecuteQuery(query))
        {
            while (reader.Read())
            {
                object nameObj = reader["AllergyName"];
                object typeObj = reader["AllergyType"];
                object categoryObj = reader["AllergyCategory"];

                string allergyName =
                    nameObj == DBNull.Value || nameObj is null
                        ? ""
                        : nameObj.ToString()!;

                string? allergyType =
                    typeObj == DBNull.Value || typeObj is null
                        ? null
                        : typeObj.ToString();

                string? allergyCategory =
                    categoryObj == DBNull.Value || categoryObj is null
                        ? null
                        : categoryObj.ToString();
                var allergy = new Allergy
                {
                    AllergyId = Convert.ToInt32(reader["AllergyID"], CultureInfo.InvariantCulture),
                    AllergyName = allergyName,
                    AllergyType = allergyType,
                    AllergyCategory = allergyCategory,
                };
                // var allergy = new Allergy
                // {
                //    AllergyId = Convert.ToInt32(reader["AllergyID"], CultureInfo.InvariantCulture),
                //    AllergyName = reader["AllergyName"].ToString() ?? "",
                //    AllergyType = reader["AllergyType"]?.ToString(),
                //    AllergyCategory = reader["AllergyCategory"]?.ToString(),
                // };
                object severityObj = reader["SeverityLevel"];

                string severity =
                    severityObj == DBNull.Value || severityObj is null
                        ? ""
                        : severityObj.ToString()!;
                result.Add((allergy, severity));
            }
        }

        return result;
    }
}
