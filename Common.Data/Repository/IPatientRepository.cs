using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public interface IPatientRepository
{
    void Add(Patient p);
    Task AddAsync(Patient p);
    void Delete(int id);
    Task DeleteAsync(int id);
    bool Exists(string cnp);
    Task<bool> ExistsAsync(string cnp);
    List<Patient> GetAll(bool include_archived);
    Task<List<Patient>> GetAllAsync(bool include_archived);
    List<Patient> GetArchived();
    Task<List<Patient>> GetArchivedAsync();
    Patient? GetById(int id);
    Task<Patient?> GetByIdAsync(int id);
    List<Patient> GetCompatibleDonors(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge);
    Task<List<Patient>> GetCompatibleDonorsAsync(BloodType bloodType, Rh rh, Sex sex, DateTime dob, int minAge, int maxAge);
    void MarkAsDeceased(int id, DateTime dod);
    Task MarkAsDeceasedAsync(int id, DateTime dod);
    List<Patient> Search(PatientFilter patientFilter);
    Task<List<Patient>> SearchAsync(PatientFilter patientFilter);
    void Update(Patient p);
    Task UpdateAsync(Patient p);
}
