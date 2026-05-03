using HospitalManagement.Entity;
using HospitalManagement.Database;
using System;
using System.Collections.Generic;
using HospitalManagement.Integration;
using System.Linq;
using HospitalManagement.Entity.DTOs;
using System.Data;
using System.Data.Common;

namespace HospitalManagement.Repository;

internal class PrescriptionRepository : IPrescriptionRepository
{
    private readonly IDbContext _context;

    public PrescriptionRepository(IDbContext context)
    {
        _context = context;
    }

    private static string Escape(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    // RP18
    public Prescription? GetByRecordId(int recordId)
    {
        if (recordId <= 0)
        {
            return null;
        }

        string sql = $"SELECT * FROM Prescription WHERE RecordID = {recordId}";

        Prescription? prescription = null;
        using (DbDataReader reader = _context.ExecuteQuery(sql))
        {
            if (!reader.Read())
            {
                return null;
            }

            prescription = new Prescription
            {
                Id = (int)reader["PrescriptionID"],
                RecordId = (int)reader["RecordID"],
                DoctorNotes = reader["DoctorNotes"] == DBNull.Value ? null : reader["DoctorNotes"].ToString(),
                Date = (DateTime)reader["Date"],
            };
        }

        // Load medication items after reader is closed
        if (prescription is not null)
        {
            prescription.MedicationList = GetItems(prescription.Id);
        }

        return prescription;
    }

    // RP19
    public void Add(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        try
        {
            _context.BeginTransaction();

            string notesValue = prescription.DoctorNotes is null
                ? "NULL"
                : $"'{Escape(prescription.DoctorNotes)}'";

            string dateValue = prescription.Date == default
                ? "CONVERT(DATE, GETDATE())"
                : $"'{FormatDate(prescription.Date)}'";

            string sqlPrescription = $@"
                    INSERT INTO Prescription (RecordID, DoctorNotes, [Date]) 
                    VALUES ({prescription.RecordId}, {notesValue}, {dateValue});
                    SELECT SCOPE_IDENTITY();";

            int newId = -1;

            using (DbDataReader reader = _context.ExecuteQuery(sqlPrescription))
            {
                if (reader.Read() && int.TryParse(reader[0].ToString(), out int id))
                {
                    newId = id;
                }
            }

            if (newId <= 0)
            {
                throw new DatabaseException("Failed to insert prescription - could not retrieve ID.");
            }

            if (prescription.MedicationList is not null)
            {
                foreach (PrescriptionItem item in prescription.MedicationList)
                {
                    string quantityValue = item.Quantity is null
                        ? "NULL"
                        : $"'{Escape(item.Quantity)}'";

                    string sqlItem = $@"
                            INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity) 
                            VALUES ({newId}, '{Escape(item.MedName)}', {quantityValue})";

                    _ = _context.ExecuteNonQuery(sqlItem);
                }
            }

            _context.CommitTransaction();
        }
        catch
        {
            _context.RollbackTransaction();
            throw;
        }
    }

    // RP19 Delete
    public void Delete(int id)
    {
        if (id <= 0)
        {
            return;
        }

        try
        {
            _context.BeginTransaction();

            _ = _context.ExecuteNonQuery(
                $"DELETE FROM PrescriptionItems WHERE PrescriptionID = {id}");

            _ = _context.ExecuteNonQuery(
                $"DELETE FROM Prescription WHERE PrescriptionID = {id}");

            _context.CommitTransaction();
        }
        catch
        {
            _context.RollbackTransaction();
            throw;
        }
    }

    // RP19 Update
    public void Update(Prescription prescription)
    {
        if (prescription is null || prescription.Id <= 0)
        {
            throw new ArgumentException("Invalid prescription data for update.");
        }

        try
        {
            _context.BeginTransaction();

            string notesValue = prescription.DoctorNotes is null
            ? "NULL"
            : $"'{Escape(prescription.DoctorNotes)}'";

            string dateValue = prescription.Date == default
                ? "CONVERT(DATE, GETDATE())"
                : $"'{FormatDate(prescription.Date)}'";

            string sqlUpdateHeader = $@"
                    UPDATE Prescription 
                    SET DoctorNotes = {notesValue}, [Date] = {dateValue}
                    WHERE PrescriptionID = {prescription.Id}";

            _ = _context.ExecuteNonQuery(sqlUpdateHeader);

            _ = _context.ExecuteNonQuery($"DELETE FROM PrescriptionItems WHERE PrescriptionID = {prescription.Id}");

            if (prescription.MedicationList is not null)
            {
                foreach (PrescriptionItem item in prescription.MedicationList)
                {
                    string quantityValue = item.Quantity is null
                        ? "NULL"
                        : $"'{Escape(item.Quantity)}'";

                    string sqlItem = $@"
                        INSERT INTO PrescriptionItems (PrescriptionID, MedName, Quantity) 
                        VALUES ({prescription.Id}, '{Escape(item.MedName)}', {quantityValue})";

                    _ = _context.ExecuteNonQuery(sqlItem);
                }
            }

            _context.CommitTransaction();
        }
        catch
        {
            _context.RollbackTransaction();
            throw;
        }
    }

    // RP21 Pagination
    public List<Prescription> GetTopN(int n, int page)
    {
        if (n <= 0)
        {
            n = 20;
        }

        if (page <= 0)
        {
            page = 1;
        }

        int offset = (page - 1) * n;

        string sql = $@"
                SELECT 
                    p.PrescriptionID, 
                    p.RecordID, 
                    p.DoctorNotes, 
                    p.[Date],
                    pat.FirstName + ' ' + pat.LastName AS PatientName
                FROM Prescription p
                JOIN MedicalRecord mr ON p.RecordID = mr.RecordID
                JOIN MedicalHistory mh ON mr.HistoryID = mh.HistoryID
                JOIN Patient pat ON mh.PatientID = pat.PatientID
                ORDER BY p.[Date] DESC, p.PrescriptionID DESC
                OFFSET {offset} ROWS FETCH NEXT {n} ROWS ONLY";

        var list = new List<Prescription>();

        using (DbDataReader reader = _context.ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                var prescription = new Prescription
                {
                    Id = (int)reader["PrescriptionID"],
                    RecordId = (int)reader["RecordID"],
                    DoctorNotes = reader["DoctorNotes"] == DBNull.Value ? null : reader["DoctorNotes"].ToString(),
                    Date = (DateTime)reader["Date"],
                    PatientName = reader["PatientName"] as string ?? "",
                };

                list.Add(prescription);
            }
        }

        foreach (Prescription rx in list)
        {
            rx.MedicationList = GetItems(rx.Id);
        }

        return list;
    }

    // RP22
    public List<PrescriptionItem> GetItems(int prescriptionId)
    {
        var items = new List<PrescriptionItem>();

        if (prescriptionId <= 0)
        {
            return items;
        }

        _context.EnsureConnectionOpen();

        string sql = $"SELECT * FROM PrescriptionItems WHERE PrescriptionID = {prescriptionId}";

        using (DbDataReader reader = _context.ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                items.Add(new PrescriptionItem
                {
                    PrescrItemId = (int)reader["PrescrItemID"],
                    PrescriptionId = (int)reader["PrescriptionID"],
                    MedName = reader["MedName"].ToString() ?? "",
                    Quantity = reader["Quantity"] == DBNull.Value
                        ? null
                        : reader["Quantity"].ToString(),
                });
            }
        }

        return items;
    }

    // RP20 Filtering
    public List<Prescription> GetFiltered(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return GetTopN(20, 1);
        }

        var list = new List<Prescription>();

        string sql = @"
                SELECT DISTINCT 
                    p.PrescriptionID, 
                    p.RecordID, 
                    p.DoctorNotes, 
                    p.[Date],
                    pat.FirstName + ' ' + pat.LastName AS PatientName
                FROM Prescription p
                LEFT JOIN PrescriptionItems pi ON p.PrescriptionID = pi.PrescriptionID
                LEFT JOIN MedicalRecord mr ON p.RecordID = mr.RecordID
                LEFT JOIN MedicalHistory mh ON mr.HistoryID = mh.HistoryID
                LEFT JOIN Patient pat ON mh.PatientID = pat.PatientID
                WHERE 1=1 AND (pat.Archived = 0 OR pat.Archived IS NULL)";

        if (filter.PrescriptionId.HasValue)
        {
            sql += $" AND p.PrescriptionID = {filter.PrescriptionId.Value}";
        }

        if (filter.PatientId.HasValue)
        {
            sql += $" AND pat.PatientID = {filter.PatientId.Value}";
        }

        if (filter.DoctorId.HasValue)
        {
            sql += $" AND mr.StaffID = {filter.DoctorId.Value}";
        }

        if (!string.IsNullOrWhiteSpace(filter.MedName))
        {
            sql += $" AND pi.MedName LIKE '%{Escape(filter.MedName)}%'";
        }

        if (filter.DateFrom.HasValue)
        {
            sql += $" AND p.[Date] >= '{FormatDate(filter.DateFrom.Value)}'";
        }

        if (filter.DateTo.HasValue)
        {
            sql += $" AND p.[Date] <= '{FormatDate(filter.DateTo.Value)}'";
        }

        if (!string.IsNullOrWhiteSpace(filter.PatientName))
        {
            string searchString = filter.PatientName;
            char[] separator = [' ',];
            string[] nameParts = Escape(searchString).Trim().Split(separator, StringSplitOptions.RemoveEmptyEntries);
            string searchTerm = searchString;

            var matchingDoctorIds = MockDoctorProvider.FakeDoctors
                .Where(d => d.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    || d.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.DoctorId)
                .ToList();

            string doctorInClause = matchingDoctorIds.Count > 0
                                  ? $"mr.StaffID IN ({string.Join(",", matchingDoctorIds)})"
                                  : "1=0";

            string patientLikeClause = "";
            if (nameParts.Length > 0)
            {
                patientLikeClause = "(";
                for (int i = 0; i < nameParts.Length; i++)
                {
                    if (i > 0)
                    {
                        patientLikeClause += " AND ";
                    }

                    patientLikeClause += $"(pat.FirstName LIKE '%{nameParts[i]}%' OR pat.LastName LIKE '%{nameParts[i]}%')";
                }

                patientLikeClause += ")";
            }

            sql += $" AND ({patientLikeClause} OR {doctorInClause})";
        }

        sql += " ORDER BY p.[Date] DESC";

        using (DbDataReader reader = _context.ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                var prescription = new Prescription
                {
                    Id = (int)reader["PrescriptionID"],
                    RecordId = (int)reader["RecordID"],
                    DoctorNotes = reader["DoctorNotes"] == DBNull.Value ? null : reader["DoctorNotes"].ToString(),
                    Date = (DateTime)reader["Date"],
                    PatientName = reader["PatientName"] as string ?? "Unknown",
                };

                list.Add(prescription);
            }
        }

        foreach (Prescription rx in list)
        {
            rx.MedicationList = GetItems(rx.Id);
        }

        return list;
    }

    public List<Prescription> GetAll()
    {
        _context.EnsureConnectionOpen();
        var prescriptions = new List<Prescription>();

        const string Query = "SELECT * FROM Prescription";

        // First pass: retrieve prescription headers without items
        using (DbDataReader reader = _context.ExecuteQuery(Query))
        {
            while (reader.Read())
            {
                var prescription = new Prescription
                {
                    Id = (int)reader["PrescriptionID"],
                    RecordId = (int)reader["RecordID"],
                    DoctorNotes = reader["DoctorNotes"] == DBNull.Value ? null : reader["DoctorNotes"].ToString(),
                    Date = (DateTime)reader["Date"],
                    MedicationList = [], // Initialize empty list
                };

                prescriptions.Add(prescription);
            }
        }

        // Second pass: load medication items for each prescription (after reader is closed)
        foreach (Prescription prescription in prescriptions)
        {
            prescription.MedicationList = GetItems(prescription.Id);
        }

        return prescriptions;
    }

    // AD1: Addict Detection Logic
    public List<Patient> GetAddictCandidatePatients()
    {
        var flagList = new List<Patient>();

        const string Sql = @"
                SELECT 
                    pat.PatientID, 
                    pat.FirstName, 
                    pat.LastName, 
                    pat.CNP, 
                    pat.DateOfBirth, 
                    pat.DateOfDeath, 
                    pat.Sex, 
                    pat.Phone, 
                    pat.EmergencyContact, 
                    pat.Archived, 
                    pat.IsDonor
                FROM Patient pat
                JOIN MedicalHistory mh ON pat.PatientID = mh.PatientID
                JOIN MedicalRecord mr ON mh.HistoryID = mr.HistoryID
                JOIN Prescription p ON mr.RecordID = p.RecordID
                JOIN PrescriptionItems pi ON p.PrescriptionID = pi.PrescriptionID
                WHERE p.[Date] >= DATEADD(day, -30, GETDATE())
                AND (pat.Archived = 0 OR pat.Archived IS NULL)
                GROUP BY 
                    pat.PatientID, pat.FirstName, pat.LastName, pat.CNP, 
                    pat.DateOfBirth, pat.DateOfDeath, pat.Sex, pat.Phone, 
                    pat.EmergencyContact, pat.Archived, pat.IsDonor, pi.MedName
                HAVING COUNT(DISTINCT mr.StaffID) >= 3";

        using (DbDataReader reader = _context.ExecuteQuery(Sql))
        {
            while (reader.Read())
            {
                flagList.Add(new Patient
                {
                    Id = (int)reader["PatientID"],
                    FirstName = reader["FirstName"].ToString() ?? "",
                    LastName = reader["LastName"].ToString() ?? "",
                    Cnp = reader["CNP"].ToString() ?? "",
                    Dob = (DateTime)reader["DateOfBirth"],
                    Dod = reader["DateOfDeath"] == DBNull.Value ? null : (DateTime)reader["DateOfDeath"],
                    Sex = Enum.Parse<Entity.Enums.Sex>(reader["Sex"].ToString() ?? "M"),
                    PhoneNo = reader["Phone"] as string ?? "",
                    EmergencyContact = reader["EmergencyContact"] as string ?? "",
                    IsArchived = (bool)reader["Archived"],
                    IsDonor = (bool)reader["IsDonor"],
                });
            }
        }

        return [.. flagList.GroupBy(p => p.Id).Select(g => g.First())];
    }
}
