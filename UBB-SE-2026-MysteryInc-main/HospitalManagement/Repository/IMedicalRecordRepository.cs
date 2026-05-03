using HospitalManagement.Entity;
using System;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface IMedicalRecordRepository
{
    public int Add(MedicalRecord record);

    public void Delete(int id);

    public List<MedicalRecord> GetAll();

    public List<MedicalRecord> GetByHistoryId(int historyId);

    public MedicalRecord? GetById(int id);

    public int? GetConsultingStaffId(int recordId);

    public int GetERVisitCount(int patientId, DateTime fromDate);

    public Prescription? GetPrescription(int recordId);

    public void Update(MedicalRecord record);
}
