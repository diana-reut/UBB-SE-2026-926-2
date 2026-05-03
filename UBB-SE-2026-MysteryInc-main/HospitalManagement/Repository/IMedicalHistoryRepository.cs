using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface IMedicalHistoryRepository
{
    public int Create(MedicalHistory history);

    public List<(Allergy Allergy, string SeverityLevel)> GetAllergiesByHistoryId(int historyId);

    public MedicalHistory? GetById(int historyId);

    public MedicalHistory? GetByPatientId(int patientId);

    public List<string> GetChronicConditions(int historyId);

    public void SaveAllergies(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies);

    public void Update(MedicalHistory history);
}
