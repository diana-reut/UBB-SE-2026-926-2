using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal interface IPatientRepository
{
    Task AddAsync(Patient p);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string cnp);
    Task<List<Patient>> GetAllAsync(bool includeArchived);
    Task<List<Patient>> GetArchivedAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<List<Patient>> GetCompatibleDonorsAsync(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge);
    Task MarkAsDeceasedAsync(int id, DateTime dod);
    Task<List<Patient>> SearchAsync(PatientFilter patientFilter);
    Task UpdateAsync(Patient p);
}