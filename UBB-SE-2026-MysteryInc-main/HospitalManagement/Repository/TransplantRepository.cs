using HospitalManagement.Entity;
using HospitalManagement.Database;
using System;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal class TransplantRepository : ITransplantRepository
{
    private readonly IDbContext _context;

    public TransplantRepository(IDbContext context)
    {
        _context = context;
    }

    private static string Escape(string value)
    {
        return value?.Replace("'", "''", StringComparison.Ordinal) ?? "";
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    // RP23: AddRequest
    public void Add(Transplant transplant)
    {
        ArgumentNullException.ThrowIfNull(transplant);

        // DECI TOATA PARTEA ASTA CU TRANSPLANT NU VA FUNCTIONA PANA NU SE SCOATE DONOR_ID NOT NULL
        // DUPA CE VA FI NULL VA FI OK ACEST COD (si requirementurile) - DONE
        string sql = $@"
            INSERT INTO Transplants (ReceiverID, DonorID, OrganType, RequestDate, TransplantDate, Status, CompatibilityScore)
            VALUES (
                {transplant.ReceiverId}, 
                NULL, 
                '{Escape(transplant.OrganType)}', 
                '{FormatDate(DateTime.Now)}', 
                NULL, 
                'Pending', 
                0
            )";

        _ = _context.ExecuteNonQuery(sql);
    }

    public List<Transplant> GetWaitingByOrgan(string organType)
    {
        string sql = $@"
            SELECT t.*, mh.BloodType, mh.RH, mh.ChronicConditions 
            FROM Transplants t
            JOIN MedicalHistory mh ON t.ReceiverID = mh.PatientID
            WHERE t.OrganType = '{Escape(organType)}' AND t.Status = 'Pending'";

        return GetListByQuery(sql);
    }


    public void Update(int id, int donorId, float score)
    {
        string sql = $@"
                UPDATE Transplants SET
                    DonorID = {donorId},
                    Status = 'Scheduled',
                    CompatibilityScore = {score.ToString(System.Globalization.CultureInfo.InvariantCulture)}
                WHERE TransplantID = {id}";

        _ = _context.ExecuteNonQuery(sql);
    }


    public List<Transplant> GetTopMatches(string organType)
    {
        string sql = $@"
                SELECT TOP 5 t.* FROM Transplants t
                WHERE t.OrganType = '{Escape(organType)}' AND t.Status = 'Pending'
                ORDER BY t.CompatibilityScore DESC";

        return GetListByQuery(sql);
    }

    public List<Transplant> GetByReceiverId(int receiverId)
    {
        return GetListByQuery($"SELECT * FROM Transplants WHERE ReceiverID = {receiverId}");
    }

    public List<Transplant> GetByDonorId(int donorId)
    {
        return GetListByQuery($"SELECT * FROM Transplants WHERE DonorID = {donorId}");
    }

    private List<Transplant> GetListByQuery(string sql)
    {
        var list = new List<Transplant>();
        using (System.Data.Common.DbDataReader reader = _context.ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                list.Add(new Transplant
                {
                    TransplantId = (int)reader["TransplantID"],
                    ReceiverId = (int)reader["ReceiverID"],
                    DonorId = reader["DonorID"] == DBNull.Value ? null : (int)reader["DonorID"],
                    OrganType = reader["OrganType"] as string ?? "",
                    RequestDate = (DateTime)reader["RequestDate"],
                    TransplantDate = reader["TransplantDate"] == DBNull.Value ? null : (DateTime)reader["TransplantDate"],
                    Status = Enum.Parse<Entity.Enums.TransplantStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                    CompatibilityScore = Convert.ToSingle(reader["CompatibilityScore"], System.Globalization.CultureInfo.InvariantCulture),
                });
            }
        }

        return list;
    }


    // Added - for transplant Service
    public Transplant? GetById(int id)
    {
        string sql = $"SELECT * FROM Transplants WHERE TransplantID = {id}";

        using System.Data.Common.DbDataReader reader = _context.ExecuteQuery(sql);
        if (reader.Read())
        {
            return new Transplant
            {
                TransplantId = (int)reader["TransplantID"],
                ReceiverId = (int)reader["ReceiverID"],
                DonorId = reader["DonorID"] == DBNull.Value ? null : (int)reader["DonorID"],
                OrganType = reader["OrganType"] as string ?? "",
                RequestDate = (DateTime)reader["RequestDate"],
                TransplantDate = reader["TransplantDate"] == DBNull.Value ? null : (DateTime)reader["TransplantDate"],
                Status = Enum.Parse<Entity.Enums.TransplantStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                CompatibilityScore = Convert.ToSingle(reader["CompatibilityScore"], System.Globalization.CultureInfo.InvariantCulture),
            };
        }

        return null;
    }
}
