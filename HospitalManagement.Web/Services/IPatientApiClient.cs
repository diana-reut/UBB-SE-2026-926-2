using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Services;

public interface IPatientApiClient
{
    Task<List<Patient>> SearchPatientsAsync(SearchPatientsDto dto, CancellationToken cancellationToken);
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Patient> GetPatientDetailsAsync(int id, CancellationToken cancellationToken);
    Task<List<string>> GetPatientAllergiesAsync(int id, CancellationToken cancellationToken);
    Task<Patient> CreatePatientAsync(CreatePatientDto dto, CancellationToken cancellationToken);
    Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryDto dto, CancellationToken cancellationToken);
    Task UpdatePatientAsync(int id, UpdatePatientDto dto, CancellationToken cancellationToken);
    Task ArchivePatientAsync(int id, CancellationToken cancellationToken);
    Task DearchivePatientAsync(int id, CancellationToken cancellationToken);
    Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedDto dto, CancellationToken cancellationToken);
    Task<bool> IsHighRiskAsync(int id, CancellationToken cancellationToken);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken);
}
