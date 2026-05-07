using Common.Data.Entity;
using Common.Data.Entity;

namespace Common.Data.Repository;

public interface IMedicalHistoryRepository
{
    int Create(MedicalHistory history);
    Task<int> CreateAsync(MedicalHistory history);
    void SaveAllergies(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies);
    Task SaveAllergiesAsync(int historyId, List<(Allergy Allergy, string SeverityLevel)> allergies);
    MedicalHistory? GetByPatientId(int patientId);
    Task<MedicalHistory?> GetByPatientIdAsync(int patientId);
    MedicalHistory? GetById(int historyId);
    Task<MedicalHistory?> GetByIdAsync(int historyId);
    void Update(MedicalHistory history);
    Task UpdateAsync(MedicalHistory history);
    List<string> GetChronicConditions(int historyId);
    Task<List<string>> GetChronicConditionsAsync(int historyId);
    List<(Allergy Allergy, string SeverityLevel)> GetAllergiesByHistoryId(int historyId);
    Task<List<(Allergy Allergy, string SeverityLevel)>> GetAllergiesByHistoryIdAsync(int historyId);
}
