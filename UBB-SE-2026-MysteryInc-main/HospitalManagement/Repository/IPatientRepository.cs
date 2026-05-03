using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface IPatientRepository
{
    public void Add(Patient p);

    public void Delete(int id);

    public bool Exists(string cnp);

    public List<Patient> GetAll(bool include_archived);

    public List<Patient> GetArchived();

    public Patient? GetById(int id);

    public void MarkAsDeceased(int id, DateOnly dod);

    public List<Patient> Search(PatientFilter patientFilter);

    public void Update(Patient patientToUpdate);
}
