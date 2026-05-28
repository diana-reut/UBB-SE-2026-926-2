using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Data.Data;

public class EFHospitalDbContext : DbContext
{
    public EFHospitalDbContext(DbContextOptions<EFHospitalDbContext> options)
        : base(options)
    {
    }
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Transplant> Transplants => Set<Transplant>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalHistory> MedicalHistory => Set<MedicalHistory>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<ER_Visit> ERVisits => Set<ER_Visit>();
    public DbSet<Triage> Triages => Set<Triage>();
    public DbSet<Triage_Parameters> TriageParameters => Set<Triage_Parameters>();
    public DbSet<ER_Room> ERRooms => Set<ER_Room>();
    public DbSet<Examination> Examinations => Set<Examination>();
    public DbSet<Transfer_Log> TransferLogs => Set<Transfer_Log>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var chronicConditionsConverter = new ValueConverter<List<string>, string>(
            value => JsonSerializer.Serialize(value ?? new List<string>(), new JsonSerializerOptions()),
            value => DeserializeChronicConditions(value));

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patient");
            entity.Property(p => p.Id).HasColumnName("PatientID");
            entity.Property(p => p.FirstName).HasColumnName("FirstName").HasMaxLength(100);
            entity.Property(p => p.LastName).HasColumnName("LastName").HasMaxLength(100);
            entity.Property(p => p.Cnp).HasColumnName("CNP").HasMaxLength(13);
            entity.Property(p => p.Dob).HasColumnName("DateOfBirth");
            entity.Property(p => p.Dod).HasColumnName("DateOfDeath");
            entity.Property(p => p.Sex)
                .HasColumnName("Sex")
                .HasConversion(
                    sex => sex == Sex.F ? "F" : "M",
                    value => value == "F" ? Sex.F : Sex.M);
            entity.Property(p => p.PhoneNo).HasColumnName("Phone").HasMaxLength(20);
            entity.Property(p => p.EmergencyContact).HasColumnName("EmergencyContact").HasMaxLength(255);
            entity.Property(p => p.IsArchived).HasColumnName("Archived");
            entity.Property(p => p.IsDonor).HasColumnName("IsDonor");
            entity.Property(p => p.Transferred).HasColumnName("Transferred");
            entity.Ignore(p => p.FullName);
            entity.Ignore(p => p.Patient_ID);
            entity.Ignore(p => p.First_Name);
            entity.Ignore(p => p.Last_Name);
            entity.Ignore(p => p.Date_of_Birth);
            entity.Ignore(p => p.Gender);
            entity.Ignore(p => p.Phone);
            entity.Ignore(p => p.Emergency_Contact);
            entity.Ignore(p => p.IsDeceased);
            entity.Ignore(p => p.IsPoliceNotified);
            entity.HasIndex(p => p.Cnp).IsUnique();
        });

        modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.ToTable("MedicalHistory");
            entity.Property(h => h.Id).HasColumnName("HistoryID");
            entity.Property(h => h.PatientId).HasColumnName("PatientID");
            entity.Property(h => h.BloodType)
                .HasColumnName("BloodType")
                .HasConversion<string?>();
            entity.Property(h => h.Rh)
                .HasColumnName("RH")
                .HasConversion<string?>();
            entity.Property(h => h.ChronicConditions)
                .HasColumnName("ChronicConditions")
                .HasConversion(chronicConditionsConverter);
            entity.Ignore(h => h.Allergies);
            entity.HasOne(h => h.Patient)
                .WithOne(p => p.MedicalHistory)
                .HasForeignKey<MedicalHistory>(h => h.PatientId);
        });

        modelBuilder.Entity<Allergy>(entity =>
        {
            entity.ToTable("Allergy");
            entity.Property(a => a.Id).HasColumnName("AllergyID");
            entity.Property(a => a.AllergyName).HasColumnName("AllergyName").HasMaxLength(100);
            entity.Property(a => a.AllergyType).HasColumnName("AllergyType").HasMaxLength(100);
            entity.Property(a => a.AllergyCategory).HasColumnName("AllergyCategory").HasMaxLength(100);
        });

        modelBuilder.Entity<PatientAllergy>(entity =>
        {
            entity.ToTable("PatientAllergies");
            entity.HasKey(pa => new { pa.AllergyId, pa.MedicalHistoryId });
            entity.Property(pa => pa.AllergyId).HasColumnName("AllergyID");
            entity.Property(pa => pa.MedicalHistoryId).HasColumnName("HistoryID");
            entity.Property(pa => pa.SeverityLevel).HasColumnName("SeverityLevel").HasMaxLength(50);
            entity.HasOne(pa => pa.MedicalHistory)
                .WithMany(h => h.PatientAllergies)
                .HasForeignKey(pa => pa.MedicalHistoryId);
            entity.HasOne(pa => pa.Allergy)
                .WithMany()
                .HasForeignKey(pa => pa.AllergyId);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.ToTable("MedicalRecord");
            entity.Property(r => r.Id).HasColumnName("RecordID");
            entity.Property(r => r.HistoryId).HasColumnName("HistoryID");
            entity.Property(r => r.SourceType)
                .HasColumnName("SourceType")
                .HasConversion(
                    value => value == SourceType.ER ? "ER Visit" : "Appointment",
                    value => value == "ER Visit" ? SourceType.ER : SourceType.App);
            entity.Property(r => r.SourceId).HasColumnName("SourceID");
            entity.Property(r => r.StaffId).HasColumnName("StaffID");
            entity.Property(r => r.Symptoms).HasColumnName("Symptoms").HasMaxLength(500);
            entity.Property(r => r.Diagnosis).HasColumnName("Diagnosis").HasMaxLength(500);
            entity.Property(r => r.ConsultationDate).HasColumnName("ConsultationDate");
            entity.Property(r => r.BasePrice).HasColumnName("BasePrice");
            entity.Property(r => r.FinalPrice).HasColumnName("FinalPrice");
            entity.Property(r => r.DiscountApplied).HasColumnName("DiscountApplied");
            entity.Property(r => r.PoliceNotified).HasColumnName("PoliceNotified");
            entity.Property(r => r.TransplantId).HasColumnName("TransplantID");
            entity.HasOne(r => r.History)
                .WithMany(h => h.MedicalRecords)
                .HasForeignKey(r => r.HistoryId);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.ToTable("Prescription");
            entity.Property(p => p.Id).HasColumnName("PrescriptionID");
            entity.Property(p => p.RecordId).HasColumnName("RecordID");
            entity.Property(p => p.DoctorNotes).HasColumnName("DoctorNotes").HasMaxLength(1000);
            entity.Property(p => p.Date).HasColumnName("Date");
            entity.Ignore(p => p.PatientName);
            entity.Ignore(p => p.DoctorName);
            entity.HasOne(p => p.MedicalRecord)
                .WithOne(r => r.Prescription)
                .HasForeignKey<Prescription>(p => p.RecordId);
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.ToTable("PrescriptionItems");
            entity.Property(pi => pi.Id).HasColumnName("PrescrItemID");
            entity.Property(pi => pi.PrescriptionId).HasColumnName("PrescriptionID");
            entity.Property(pi => pi.MedName).HasColumnName("MedName").HasMaxLength(150);
            entity.Property(pi => pi.Quantity).HasColumnName("Quantity").HasMaxLength(50);
        });

        modelBuilder.Entity<Transplant>(entity =>
        {
            entity.ToTable("Transplants");
            entity.Property(t => t.TransplantId).HasColumnName("TransplantID");
            entity.Property(t => t.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(t => t.DonorId).HasColumnName("DonorID");
            entity.Property(t => t.OrganType).HasColumnName("OrganType").HasMaxLength(100);
            entity.Property(t => t.RequestDate).HasColumnName("RequestDate");
            entity.Property(t => t.TransplantDate).HasColumnName("TransplantDate");
            entity.Property(t => t.Status).HasColumnName("Status").HasConversion<string>();
            entity.Property(t => t.CompatibilityScore).HasColumnName("CompatibilityScore");
        });

        modelBuilder.Entity<ER_Visit>(entity =>
        {
            entity.ToTable("ER_Visit");
            entity.HasKey(v => v.Visit_ID);
            entity.Property(v => v.Visit_ID).HasColumnName("Visit_ID");
            entity.Property(v => v.Patient_ID).HasColumnName("Patient_ID").HasMaxLength(13);
            entity.Property(v => v.Arrival_date_time).HasColumnName("Arrival_date_time");
            entity.Property(v => v.Chief_Complaint).HasColumnName("Chief_Complaint").HasMaxLength(255);
            entity.Property(v => v.Status).HasColumnName("Status").HasMaxLength(30);
        });

        modelBuilder.Entity<Triage>(entity =>
        {
            entity.ToTable("Triage");
            entity.HasKey(t => t.Triage_ID);
            entity.Property(t => t.Triage_ID).HasColumnName("Triage_ID");
            entity.Property(t => t.Visit_ID).HasColumnName("Visit_ID");
            entity.Property(t => t.Triage_Level).HasColumnName("Triage_Level");
            entity.Property(t => t.Specialization).HasColumnName("Specialization").HasMaxLength(50);
            entity.Property(t => t.Nurse_ID).HasColumnName("Nurse_ID");
            entity.Property(t => t.Triage_Time).HasColumnName("Triage_Time");
        });

        modelBuilder.Entity<Triage_Parameters>(entity =>
        {
            entity.ToTable("Triage_Parameters");
            entity.HasKey(tp => tp.TriageParametersId);
            entity.Property(tp => tp.TriageParametersId)
                .HasColumnName("Triage_ID")
                .ValueGeneratedOnAdd();
            entity.Property(tp => tp.TriageId).HasColumnName("TriageId");
            entity.Ignore(tp => tp.Triage_ID);
            entity.HasOne<Triage>()
                .WithOne()
                .HasForeignKey<Triage_Parameters>(tp => tp.TriageId)
                .HasPrincipalKey<Triage>(t => t.Triage_ID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(tp => tp.TriageId).IsUnique();
            entity.Property(tp => tp.Consciousness).HasColumnName("Consciousness");
            entity.Property(tp => tp.Breathing).HasColumnName("Breathing");
            entity.Property(tp => tp.Bleeding).HasColumnName("Bleeding");
            entity.Property(tp => tp.Injury_Type).HasColumnName("Injury_Type");
            entity.Property(tp => tp.Pain_Level).HasColumnName("Pain_Level");
        });

        modelBuilder.Entity<ER_Room>(entity =>
        {
            entity.ToTable("ER_Room");
            entity.HasKey(r => r.Room_ID);
            entity.Property(r => r.Room_ID).HasColumnName("Room_ID");
            entity.Property(r => r.Room_Type).HasColumnName("Room_Type").HasMaxLength(50);
            entity.Property(r => r.Availability_Status).HasColumnName("Availability_Status").HasMaxLength(50);
            entity.Property(r => r.Current_Visit_ID).HasColumnName("Current_Visit_ID");
        });

        modelBuilder.Entity<Examination>(entity =>
        {
            entity.ToTable("Examination");
            entity.HasKey(e => e.Exam_ID);
            entity.Property(e => e.Exam_ID).HasColumnName("Exam_ID");
            entity.Property(e => e.Visit_ID).HasColumnName("Visit_ID");
            entity.Property(e => e.Doctor_ID).HasColumnName("Doctor_ID");
            entity.Property(e => e.Exam_Time).HasColumnName("Exam_Time");
            entity.Property(e => e.Room_ID).HasColumnName("Room_ID");
            entity.Property(e => e.Notes).HasColumnName("Notes").HasMaxLength(1000);
        });

        modelBuilder.Entity<Transfer_Log>(entity =>
        {
            entity.ToTable("Transfer_Log");
            entity.HasKey(t => t.Transfer_ID);
            entity.Property(t => t.Transfer_ID).HasColumnName("Transfer_ID");
            entity.Property(t => t.Visit_ID).HasColumnName("Visit_ID");
            entity.Property(t => t.Transfer_Time).HasColumnName("Transfer_Time");
            entity.Property(t => t.Target_System).HasColumnName("Target_System").HasMaxLength(30);
            entity.Property(t => t.Status).HasColumnName("Status").HasMaxLength(30);
            entity.Property(t => t.FilePath).HasColumnName("FilePath").HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }

    private static List<string> DeserializeChronicConditions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        string trimmed = value.Trim();
        if (trimmed.StartsWith('['))
        {
            return JsonSerializer.Deserialize<List<string>>(trimmed, new JsonSerializerOptions()) ?? new List<string>();
        }

        return trimmed
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(condition => !string.IsNullOrWhiteSpace(condition))
            .ToList();
    }
}
