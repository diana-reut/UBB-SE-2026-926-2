using Microsoft.EntityFrameworkCore;
using HospitalManagement.Entity;
using Common.Data.Entity;

namespace HospitalManagement.Data
{
    public class EFHospitalDbContext : DbContext
    {
        public EFHospitalDbContext(DbContextOptions<EFHospitalDbContext> options) : base(options)
        {
        }
        public EFHospitalDbContext() { }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Transplant> Transplants { get; set; }
        public DbSet<TransplantMatch> TransplantMatches { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<MedicalHistory> MedicalHistory { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        public DbSet<PatientAllergy> PatientAllergies { get; set; }

        public DbSet<Allergy> Allergies { get; set; }

    }
}