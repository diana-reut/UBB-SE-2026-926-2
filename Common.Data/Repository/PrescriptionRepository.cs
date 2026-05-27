using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Data;
using Common.Data.Entity;
using Common.Data.Entity;
using Common.Data.Integration;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly EFHospitalDbContext context;

    public PrescriptionRepository(EFHospitalDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);
        await context.Prescriptions.AddAsync(prescription);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        Prescription existing = await context.Prescriptions
            .Include(p => p.MedicationList)
            .FirstOrDefaultAsync(p => p.Id == prescription.Id)
            ?? throw new KeyNotFoundException();

        context.Entry(existing).CurrentValues.SetValues(prescription);
        context.PrescriptionItems.RemoveRange(existing.MedicationList);
        existing.MedicationList.Clear();

        foreach (PrescriptionItem item in prescription.MedicationList)
        {
            existing.MedicationList.Add(item);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Prescription? prescription = await context.Prescriptions.FindAsync(id);
        if (prescription is not null)
        {
            context.Prescriptions.Remove(prescription);
            await context.SaveChangesAsync();
        }
    }

    public Task<List<Prescription>> GetTopNAsync(int n, int page)
    {
        int pageSize = n <= 0 ? 20 : n;
        int pageNumber = page <= 0 ? 1 : page;

        return BaseQuery()
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId) =>
        context.PrescriptionItems
            .Where(pi => pi.PrescriptionId == prescriptionId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return await GetTopNAsync(20, 1);
        }

        IQueryable<Prescription> query = BaseQuery()
            .Where(p => !p.MedicalRecord.History.Patient.IsArchived);

        if (filter.PrescriptionId.HasValue)
        {
            query = query.Where(p => p.Id == filter.PrescriptionId.Value);
        }

        if (filter.PatientId.HasValue)
        {
            query = query.Where(p => p.MedicalRecord.History.PatientId == filter.PatientId.Value);
        }

        if (filter.DoctorId.HasValue)
        {
            query = query.Where(p => p.MedicalRecord.StaffId == filter.DoctorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.MedName))
        {
            query = query.Where(p => p.MedicationList.Any(m => m.MedName.Contains(filter.MedName)));
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(p => p.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(p => p.Date <= filter.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PatientName))
        {
            query = query.Where(p =>
                p.MedicalRecord.History.Patient.FirstName.Contains(filter.PatientName) ||
                p.MedicalRecord.History.Patient.LastName.Contains(filter.PatientName));
        }

        return await query
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public Task<List<Prescription>> GetAllAsync() =>
        BaseQuery().ToListAsync();

    public Task<Prescription?> GetByRecordIdAsync(int recordId) =>
        BaseQuery().FirstOrDefaultAsync(p => p.RecordId == recordId);

    public List<Patient> GetAddictCandidatePatients() => GetAddictCandidatePatientsAsync().GetAwaiter().GetResult();

    public async Task<List<Patient>> GetAddictCandidatePatientsAsync()
    {
        DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

        var candidateIds = new List<int>();
        var conn = context.Database.GetDbConnection();
        bool wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
        {
            await conn.OpenAsync();
        }

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT mh.PatientID
                FROM PrescriptionItems pi
                INNER JOIN Prescription pr ON pi.PrescriptionID = pr.PrescriptionID
                INNER JOIN MedicalRecord mr ON pr.RecordID = mr.RecordID
                INNER JOIN MedicalHistory mh ON mr.HistoryID = mh.HistoryID
                INNER JOIN Patient p ON mh.PatientID = p.PatientID
                WHERE pr.Date >= @thirtyDaysAgo
                  AND p.Archived = 0
                GROUP BY mh.PatientID, pi.MedName
                HAVING COUNT(DISTINCT mr.StaffID) >= 3";

            var param = cmd.CreateParameter();
            param.ParameterName = "@thirtyDaysAgo";
            param.Value = thirtyDaysAgo;
            cmd.Parameters.Add(param);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                candidateIds.Add(reader.GetInt32(0));
            }
        }
        finally
        {
            if (!wasOpen)
            {
                await conn.CloseAsync();
            }
        }

        if (candidateIds.Count == 0)
        {
            return new List<Patient>();
        }

        return await context.Patients
            .Where(p => candidateIds.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkPoliceNotifiedAsync(int patientId)
    {
        DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

        List<MedicalRecord> records = await context.Prescriptions
            .Where(p => p.MedicalRecord.History.PatientId == patientId && p.Date >= thirtyDaysAgo)
            .Select(p => p.MedicalRecord)
            .ToListAsync();

        foreach (MedicalRecord record in records)
        {
            record.PoliceNotified = true;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds)
    {
        DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

        return await context.Prescriptions
            .Where(p => patientIds.Contains(p.MedicalRecord.History.PatientId)
                        && p.Date >= thirtyDaysAgo
                        && p.MedicalRecord.PoliceNotified)
            .Select(p => p.MedicalRecord.History.PatientId)
            .Distinct()
            .ToListAsync();
    }

    private IQueryable<Prescription> BaseQuery() =>
        context.Prescriptions
            .Include(p => p.MedicationList)
            .Include(p => p.MedicalRecord)
            .ThenInclude(mr => mr.History)
            .ThenInclude(mh => mh.Patient)
            .AsNoTracking();
}
