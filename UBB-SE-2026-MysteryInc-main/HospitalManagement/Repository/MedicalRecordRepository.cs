using System;
using System.Collections.Generic;
using HospitalManagement.Database;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using System.Data.Common;

namespace HospitalManagement.Repository;

internal class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly IDbContext _context;

    public MedicalRecordRepository(IDbContext context)
    {
        _context = context;
    }

    public List<MedicalRecord> GetByHistoryId(int historyId) // RP15
    {
        var records = new List<MedicalRecord>();
        string query = $"SELECT * FROM MedicalRecord WHERE HistoryID={historyId}";
        using (DbDataReader reader = _context.ExecuteQuery(query))
        {
            while (reader.Read())
            {
                records.Add(MapToMedicalRecord(reader));
            }
        }

        return records;
    }

    public MedicalRecord? GetById(int id) // RP15
    {
        string query = $"SELECT * FROM MedicalRecord WHERE RecordID={id}";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return MapToMedicalRecord(reader);
        }

        return null;
    }

    public int Add(MedicalRecord record) // RP16
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record), "MedicalRecord cannot be null.");
        }

        if (record.HistoryId <= 0 || record.StaffId <= 0)
        {
            throw new ArgumentException("HistoryId and StaffId must be greater than 0.");
        }

        // Convert enum to database string values
        string sourceTypeStr = record.SourceType switch
        {
            SourceType.ER => "ER Visit",
            SourceType.App => "Appointment",
            SourceType.Admin => "Admin",
            _ => throw new InvalidOperationException($"Unknown SourceType: {record.SourceType}"),
        };

        string query = $@"
        INSERT INTO MedicalRecord
        (HistoryID, SourceType, SourceID, StaffID, Symptoms, Diagnosis, ConsultationDate,
         BasePrice, FinalPrice, DiscountApplied, PoliceNotified, TransplantID)
        OUTPUT INSERTED.RecordID
        VALUES
        ({record.HistoryId}, '{sourceTypeStr}', {record.SourceId}, {record.StaffId},
         {(record.Symptoms is not null ? $"'{record.Symptoms}'" : "NULL")},
         {(record.Diagnosis is not null ? $"'{record.Diagnosis}'" : "NULL")},
         '{record.ConsultationDate:yyyy-MM-dd HH:mm:ss}',
         {record.BasePrice}, {record.FinalPrice},
         {(record.DiscountApplied.HasValue ? record.DiscountApplied.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")},
         {(record.PoliceNotified ? 1 : 0)},
         {(record.TransplantId.HasValue ? record.TransplantId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")}
        )";

        using (DbDataReader reader = _context.ExecuteQuery(query))
        {
            if (reader.Read())
            {
                return reader.GetInt32(reader.GetOrdinal("RecordID"));
            }
        }

        throw new DatabaseException("Failed to insert MedicalRecord.");
    }

    public void Update(MedicalRecord record) // RP16
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record), "MedicalRecord cannot be null.");
        }

        // ID range validation
        if (record.Id <= 0 || record.HistoryId <= 0 || record.StaffId <= 0)
        {
            throw new ArgumentException("Id, HistoryId and StaffId must be greater than 0.");
        }

        // Verify historyId exists to prevent orphaned records
        string checkQuery = $"SELECT COUNT(*) FROM MedicalHistory WHERE HistoryID={record.HistoryId}";
        using (DbDataReader reader = _context.ExecuteQuery(checkQuery))
        {
            if (reader.Read() && reader.GetInt32(0) == 0)
            {
                throw new KeyNotFoundException($"No MedicalHistory found with ID={record.HistoryId}.");
            }
        }

        string query = "UPDATE MedicalRecord SET "
            + $"Symptoms = {(record.Symptoms is not null ? $"'{record.Symptoms}'" : "NULL")}, "
            + $"Diagnosis = {(record.Diagnosis is not null ? $"'{record.Diagnosis}'" : "NULL")}, "
            + $"BasePrice = {record.BasePrice}, "
            + $"FinalPrice = {record.FinalPrice}, "
            + $"DiscountApplied = {(record.DiscountApplied.HasValue ? record.DiscountApplied.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")}, "
            + $"PoliceNotified = {(record.PoliceNotified ? 1 : 0)}, "
            + $"TransplantID = {(record.TransplantId.HasValue ? record.TransplantId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")} "
            + $"WHERE RecordID = {record.Id}";
        _ = _context.ExecuteNonQuery(query);
    }

    public void Delete(int id) // RP16
    {
        // Void associated prescription before deleting the record
        // A prescription cannot exist without its parent consultation
        string deletePrescription = $"DELETE FROM Prescription WHERE RecordID={id}";
        _ = _context.ExecuteNonQuery(deletePrescription);

        string query = $"DELETE FROM MedicalRecord WHERE RecordID={id}";
        _ = _context.ExecuteNonQuery(query);
    }

    public int GetERVisitCount(int patientId, DateTime fromDate) // RP17
    {
        string query = "SELECT COUNT(*) FROM MedicalRecord mr "
            + "JOIN MedicalHistory mh ON mr.HistoryID = mh.HistoryID "
            + $"WHERE mh.PatientID = {patientId} "
            + "AND mr.SourceType = 'ER Visit' "
            + $"AND mr.ConsultationDate >= '{fromDate:yyyy-MM-dd}'";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }

        return 0;
    }

    private static MedicalRecord MapToMedicalRecord(DbDataReader reader)
    {
        // Convert database string values to enum
        // Database stores: 'ER Visit' → ER, 'Appointment' → App
        string sourceTypeStr = reader["SourceType"].ToString()!;
        SourceType sourceType = sourceTypeStr switch
        {
            "ER Visit" => SourceType.ER,
            "Appointment" => SourceType.App,
            "Admin" => SourceType.Admin,
            _ => throw new InvalidOperationException($"Unknown SourceType value in database: {sourceTypeStr}"),
        };

        return new MedicalRecord
        {
            Id = reader.GetInt32(reader.GetOrdinal("RecordID")),
            HistoryId = reader.GetInt32(reader.GetOrdinal("HistoryID")),
            SourceType = sourceType,
            SourceId = reader.GetInt32(reader.GetOrdinal("SourceID")),
            StaffId = reader.GetInt32(reader.GetOrdinal("StaffID")),

            Symptoms = reader["Symptoms"] as string,
            Diagnosis = reader["Diagnosis"] as string,

            ConsultationDate = reader.GetDateTime(reader.GetOrdinal("ConsultationDate")),
            PrescriptionId = null,

            BasePrice = reader.GetDecimal(reader.GetOrdinal("BasePrice")),
            FinalPrice = reader.GetDecimal(reader.GetOrdinal("FinalPrice")),

            DiscountApplied = reader.IsDBNull(reader.GetOrdinal("DiscountApplied"))
                      ? null
                      : reader.GetInt32(reader.GetOrdinal("DiscountApplied")),

            PoliceNotified = reader.GetBoolean(reader.GetOrdinal("PoliceNotified")),

            TransplantId = reader.IsDBNull(reader.GetOrdinal("TransplantID"))
                   ? null
                   : reader.GetInt32(reader.GetOrdinal("TransplantID")),
        };
    }



    // added things from MedicalRecord ENtity

    public Prescription? GetPrescription(int recordId) // getPrescription()
    {
        // PrescriptionRepository will own the full mapping,
        // but MedicalRecord needs basic navigation — we do a lightweight fetch here
        string query = $"SELECT * FROM Prescription WHERE RecordID={recordId}";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return new Prescription
            {
                Id = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                RecordId = reader.GetInt32(reader.GetOrdinal("RecordID")),
                DoctorNotes = reader["DoctorNotes"]?.ToString(),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
            };
        }

        return null;
    }

    public int? GetConsultingStaffId(int recordId) // getConsultingStaff()
    {
        // Staff details live in an external module (StaffProxy handles the full object).
        // The repo's job is just to return the StaffID so the service/proxy can fetch the rest.
        string query = $"SELECT StaffID FROM MedicalRecord WHERE RecordID={recordId}";
        using DbDataReader reader = _context.ExecuteQuery(query);
        if (reader.Read())
        {
            return reader.GetInt32(reader.GetOrdinal("StaffID"));
        }

        return null;
    }


    public List<MedicalRecord> GetAll() // for statistics service
    {
        _context.EnsureConnectionOpen();
        var records = new List<MedicalRecord>();
        const string Query = "SELECT * FROM MedicalRecord";
        using (DbDataReader reader = _context.ExecuteQuery(Query))
        {
            while (reader.Read())
            {
                records.Add(MapToMedicalRecord(reader));
            }
        }

        return records;
    }
}
