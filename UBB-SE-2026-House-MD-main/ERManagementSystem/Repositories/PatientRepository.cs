using System.Linq;
using System.Threading.Tasks;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using HospitalManagement.Data;
using Microsoft.EntityFrameworkCore;
using PatientEntity = HospitalManagement.Entity.Patient;

namespace ERManagementSystem.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EFHospitalDbContext context;

        public PatientRepository(EFHospitalDbContext context)
        {
            this.context = context;
        }

        public void Add(Patient patient)
            => AddAsync(patient).GetAwaiter().GetResult();

        public async Task AddAsync(Patient patient)
        {
            PatientEntity entity = new ()
            {
                FirstName = patient.First_Name,
                LastName = patient.Last_Name,
                Cnp = patient.Patient_ID,
                Dob = patient.Date_of_Birth,
                Sex = patient.Gender == "Female" ? HospitalManagement.Entity.Enums.Sex.F : HospitalManagement.Entity.Enums.Sex.M,
                PhoneNo = patient.Phone,
                EmergencyContact = patient.Emergency_Contact,
                Transferred = patient.Transferred,
                IsArchived = false,
                IsDonor = false,
            };

            await context.Patients.AddAsync(entity);
            await context.SaveChangesAsync();
            Logger.Info($"Patient {patient.Patient_ID} added through EF Core.");
        }

        public Patient? GetById(string id)
            => GetByIdAsync(id).GetAwaiter().GetResult();

        public async Task<Patient?> GetByIdAsync(string id)
        {
            PatientEntity? entity = await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Cnp == id);

            return entity is null ? null : MapToModel(entity);
        }

        public void Update(Patient patient)
            => UpdateAsync(patient).GetAwaiter().GetResult();

        public async Task UpdateAsync(Patient patient)
        {
            PatientEntity entity = await context.Patients.FirstAsync(p => p.Cnp == patient.Patient_ID);
            entity.FirstName = patient.First_Name;
            entity.LastName = patient.Last_Name;
            entity.Dob = patient.Date_of_Birth;
            entity.Sex = patient.Gender == "Female" ? HospitalManagement.Entity.Enums.Sex.F : HospitalManagement.Entity.Enums.Sex.M;
            entity.PhoneNo = patient.Phone;
            entity.EmergencyContact = patient.Emergency_Contact;
            entity.Transferred = patient.Transferred;
            await context.SaveChangesAsync();
            Logger.Info($"Patient {patient.Patient_ID} updated through EF Core.");
        }

        public void Delete(Patient patient)
            => DeleteAsync(patient).GetAwaiter().GetResult();

        public async Task DeleteAsync(Patient patient)
        {
            PatientEntity? entity = await context.Patients.FirstOrDefaultAsync(p => p.Cnp == patient.Patient_ID);
            if (entity is not null)
            {
                context.Patients.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        private static Patient MapToModel(PatientEntity entity) =>
            new ()
            {
                Patient_ID = entity.Cnp,
                First_Name = entity.FirstName,
                Last_Name = entity.LastName,
                Date_of_Birth = entity.Dob,
                Gender = entity.Sex == HospitalManagement.Entity.Enums.Sex.F ? "Female" : "Male",
                Phone = entity.PhoneNo,
                Emergency_Contact = entity.EmergencyContact,
                Transferred = entity.Transferred,
            };
    }
}
