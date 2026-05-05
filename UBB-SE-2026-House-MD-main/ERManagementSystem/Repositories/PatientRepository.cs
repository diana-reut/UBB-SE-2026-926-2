using System.Linq;
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

            context.Patients.Add(entity);
            context.SaveChanges();
            Logger.Info($"Patient {patient.Patient_ID} added through EF Core.");
        }

        public Patient? GetById(string id)
        {
            PatientEntity? entity = context.Patients
                .AsNoTracking()
                .FirstOrDefault(p => p.Cnp == id);

            return entity is null ? null : MapToModel(entity);
        }

        public void Update(Patient patient)
        {
            PatientEntity entity = context.Patients.First(p => p.Cnp == patient.Patient_ID);
            entity.FirstName = patient.First_Name;
            entity.LastName = patient.Last_Name;
            entity.Dob = patient.Date_of_Birth;
            entity.Sex = patient.Gender == "Female" ? HospitalManagement.Entity.Enums.Sex.F : HospitalManagement.Entity.Enums.Sex.M;
            entity.PhoneNo = patient.Phone;
            entity.EmergencyContact = patient.Emergency_Contact;
            entity.Transferred = patient.Transferred;
            context.SaveChanges();
            Logger.Info($"Patient {patient.Patient_ID} updated through EF Core.");
        }

        public void Delete(Patient patient)
        {
            PatientEntity? entity = context.Patients.FirstOrDefault(p => p.Cnp == patient.Patient_ID);
            if (entity is not null)
            {
                context.Patients.Remove(entity);
                context.SaveChanges();
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
