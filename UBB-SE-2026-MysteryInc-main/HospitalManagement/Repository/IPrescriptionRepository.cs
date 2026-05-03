using HospitalManagement.Entity;
using HospitalManagement.Integration;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface IPrescriptionRepository
{
    public void Add(Prescription prescription);

    public void Delete(int id);

    public List<Patient> GetAddictCandidatePatients();

    public List<Prescription> GetAll();

    public Prescription? GetByRecordId(int recordId);

    public List<Prescription> GetFiltered(PrescriptionFilter filter);

    public List<PrescriptionItem> GetItems(int prescriptionId);

    public List<Prescription> GetTopN(int n, int page);

    public void Update(Prescription prescription);
}
