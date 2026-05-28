using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Common.Data.Entity;
using Common.Data.Integration;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/patients")]
[AuthorizeRole("Admin", "Medic")]
public class PatientController : ControllerBase
{
    private readonly IPatientService patientService;
    private readonly ILogger<PatientController> logger;

    public PatientController(IPatientService patientService, ILogger<PatientController> logger)
    {
        this.patientService = patientService;
        this.logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetById(int id)
    {
        try
        {
            Patient? patient = await patientService.GetByIdAsync(id);
            if (patient is null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch patient with id {Id}.", id);
            return Problem(
                detail: $"Failed to fetch patient with id {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch patient.");
        }
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<Patient>> GetPatientDetails(int id)
    {
        try
        {
            Patient patient = await patientService.GetPatientDetailsAsync(id);
            return Ok(patient);
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Patient with id {Id} not found.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch details for patient {Id}.", id);
            return Problem(
                detail: $"Failed to fetch details for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch patient details.");
        }
    }

    [HttpGet("{id}/medical-history")]
    public async Task<ActionResult<MedicalHistory>> GetMedicalHistory(int id)
    {
        try
        {
            MedicalHistory? history = await patientService.GetMedicalHistoryAsync(id);
            if (history is null)
            {
                return NotFound();
            }

            return Ok(history);
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Invalid patient id {Id} when fetching medical history.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch medical history for patient {Id}.", id);
            return Problem(
                detail: $"Failed to fetch medical history for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch medical history.");
        }
    }

    [HttpGet("{id}/medical-records")]
    public async Task<ActionResult<List<MedicalRecord>>> GetMedicalRecords(int id)
    {
        try
        {
            List<MedicalRecord> records = await patientService.GetMedicalRecordsAsync(id);
            return Ok(records);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch medical records for history {Id}.", id);
            return Problem(
                detail: $"Failed to fetch medical records for history id {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch medical records.");
        }
    }

    [HttpGet("{id}/allergies")]
    public async Task<ActionResult<List<string>>> GetPatientAllergies(int id)
    {
        try
        {
            List<string> allergies = await patientService.GetPatientAllergiesAsync(id);
            return Ok(allergies);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch allergies for patient {Id}.", id);
            return Problem(
                detail: $"Failed to fetch allergies for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch patient allergies.");
        }
    }

    [HttpGet("{id}/high-risk")]
    public async Task<ActionResult<bool>> IsHighRiskPatient(int id)
    {
        try
        {
            bool isHighRisk = await patientService.IsHighRiskPatientAsync(id);
            return Ok(isHighRisk);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to evaluate high-risk status for patient {Id}.", id);
            return Problem(
                detail: $"Failed to evaluate high-risk status for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not evaluate high-risk status.");
        }
    }

    [HttpGet("exists/{cnp}")]
    public async Task<ActionResult<bool>> Exists(string cnp)
    {
        try
        {
            bool exists = await patientService.ExistsAsync(cnp);
            return Ok(exists);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to check existence for CNP {Cnp}.", cnp);
            return Problem(
                detail: $"Failed to check existence for CNP {cnp}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not check patient existence.");
        }
    }

    [HttpGet("records/{recordId}/prescription")]
    public async Task<ActionResult<Prescription>> GetPrescriptionByRecordId(int recordId)
    {
        try
        {
            Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId);
            if (prescription is null)
            {
                return NotFound();
            }

            return Ok(prescription);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Prescription repository unavailable.");
            return Problem(
                detail: e.Message,
                statusCode: (int)HttpStatusCode.ServiceUnavailable,
                title: "Prescription service unavailable.");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch prescription for record {RecordId}.", recordId);
            return Problem(
                detail: $"Failed to fetch prescription for record {recordId}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch prescription.");
        }
    }

    [HttpPost]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult<Patient>> CreatePatient([FromBody] CreatePatientDto dto)
    {
        try
        {
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Cnp = dto.Cnp,
                Dob = dto.Dob,
                Sex = dto.Sex,
                PhoneNo = dto.PhoneNo,
                EmergencyContact = dto.EmergencyContact,
                IsDonor = dto.IsDonor,
                IsArchived = false,
                Transferred = false,
            };

            Patient created = await patientService.CreatePatientAsync(patient);
            return Ok(created);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Validation failed when creating patient.");
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create patient.");
            return Problem(
                detail: "Failed to create patient.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not create patient.");
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<Patient>>> SearchPatients([FromBody] SearchPatientsDto dto)
    {
        try
        {
            var filter = new PatientFilter
            {
                NamePart = dto.NamePart,
                CNP = dto.Cnp,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                Sex = dto.Sex,
                HasChronicCond = dto.HasChronicCond,
                LastUpdatedFrom = dto.LastUpdatedFrom,
                LastUpdatedTo = dto.LastUpdatedTo,
                BloodType = dto.BloodType,
                Rh = dto.Rh,
            };

            List<Patient> results = await patientService.SearchPatientsAsync(filter);
            return Ok(results);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid search filter provided.");
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to search patients.");
            return Problem(
                detail: "Failed to search patients.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not search patients.");
        }
    }

    [HttpPost("{id}/medical-history")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> CreateMedicalHistory(int id, [FromBody] CreateMedicalHistoryDto dto)
    {
        try
        {
            var history = new MedicalHistory
            {
                BloodType = dto.BloodType,
                Rh = dto.Rh,
                ChronicConditions = dto.ChronicConditions,
                PatientAllergies = dto.AllergyIds
                    .Distinct()
                    .Select(allergyId => new PatientAllergy
                    {
                        AllergyId = allergyId,
                        SeverityLevel = "Mild"
                    })
                    .ToList()
            };

            await patientService.CreateMedicalHistoryAsync(id, history);
            return Ok();
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Validation failed when creating medical history for patient {Id}.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create medical history for patient {Id}.", id);
            return Problem(
                detail: $"Failed to create medical history for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not create medical history.");
        }
    }

    [HttpPost("{id}/medical-records")]
    [AuthorizeRole("Admin", "Medic")]
    public async Task<ActionResult<int>> CreateMedicalRecord(int id, [FromBody] CreateMedicalRecordDto dto)
    {
        try
        {
            var record = new MedicalRecord
            {
                SourceType = dto.SourceType,
                SourceId = dto.SourceId,
                StaffId = dto.StaffId,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                ConsultationDate = dto.ConsultationDate,
                BasePrice = dto.BasePrice,
                FinalPrice = dto.FinalPrice,
                PoliceNotified = dto.PoliceNotified
            };

            int recordId = await patientService.CreateMedicalRecordAsync(id, record);
            return Ok(recordId);
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Patient {Id} not found when creating medical record.", id);
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Patient {Id} has no medical history when creating record.", id);
            return Problem(
                detail: e.Message,
                statusCode: (int)HttpStatusCode.Conflict,
                title: "Could not create medical record.");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create medical record for patient {Id}.", id);
            return Problem(
                detail: $"Failed to create medical record for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not create medical record.");
        }
    }

    [HttpPost("records/{recordId}/prescription")]
    [AuthorizeRole("Admin", "Medic")]
    public async Task<ActionResult> CreatePrescriptionForRecord(int recordId, [FromBody] CreatePrescriptionDto dto)
    {
        try
        {
            var prescription = new Prescription
            {
                DoctorNotes = dto.DoctorNotes,
                Date = dto.Date,
                MedicationList = dto.Items
                    .Select(item => new PrescriptionItem
                    {
                        MedName = item.MedName,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };

            await patientService.CreatePrescriptionAsync(recordId, prescription);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Medical record {RecordId} not found when creating prescription.", recordId);
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Prescription repository unavailable when creating prescription for record {RecordId}.", recordId);
            return Problem(
                detail: e.Message,
                statusCode: (int)HttpStatusCode.ServiceUnavailable,
                title: "Could not create prescription.");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create prescription for record {RecordId}.", recordId);
            return Problem(
                detail: $"Failed to create prescription for record {recordId}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not create prescription.");
        }
    }

    [HttpGet("records/{recordId}/export-data")]
    public async Task<ActionResult<RecordExportDataDto>> GetRecordExportData(int recordId)
    {
        try
        {
            RecordExportDataDto result = await patientService.GetRecordExportDataAsync(recordId);
            return Ok(result);
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Export data for medical record {RecordId} not found.", recordId);
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Export data unavailable for medical record {RecordId}.", recordId);
            return Problem(
                detail: e.Message,
                statusCode: (int)HttpStatusCode.ServiceUnavailable,
                title: "Could not fetch export data.");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch export data for medical record {RecordId}.", recordId);
            return Problem(
                detail: $"Failed to fetch export data for medical record {recordId}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch export data.");
        }
    }

    [HttpPut("{id}")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
    {
        try
        {
            var patient = new Patient
            {
                Id = id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Cnp = dto.Cnp,
                Dob = dto.Dob,
                Dod = dto.Dod.HasValue && dto.Dod.Value > DateTime.MinValue ? dto.Dod : null,
                Sex = dto.Sex,
                PhoneNo = dto.PhoneNo,
                EmergencyContact = dto.EmergencyContact,
                IsArchived = dto.IsArchived,
                IsDonor = dto.IsDonor,
                Transferred = dto.Transferred,
            };

            await patientService.UpdatePatientAsync(patient);
            return Ok();
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Validation failed when updating patient {Id}.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to update patient {Id}.", id);
            return Problem(
                detail: $"Failed to update patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not update patient.");
        }
    }

    [HttpPut("{id}/archive")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> ArchivePatient(int id)
    {
        try
        {
            Patient? patient = await patientService.GetByIdAsync(id);
            if (patient is null)
            {
                return NotFound();
            }

            await patientService.ArchivePatientAsync(patient);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to archive patient {Id}.", id);
            return Problem(
                detail: $"Failed to archive patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not archive patient.");
        }
    }

    [HttpPut("{id}/dearchive")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> DearchivePatient(int id)
    {
        try
        {
            await patientService.DearchivePatientAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Patient {Id} not found when dearchiving.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to dearchive patient {Id}.", id);
            return Problem(
                detail: $"Failed to dearchive patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not dearchive patient.");
        }
    }

    [HttpPut("{id}/archive-deceased")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> ArchiveAsDeceased(int id, [FromBody] ArchiveAsDeceasedDto dto)
    {
        try
        {
            await patientService.ArchiveAsDeceasedAsync(id, dto.DeathDate);
            return Ok();
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Validation failed when archiving patient {Id} as deceased.", id);
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Patient {Id} not found when archiving as deceased.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to archive patient {Id} as deceased.", id);
            return Problem(
                detail: $"Failed to archive patient {id} as deceased.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not archive patient as deceased.");
        }
    }

    [HttpDelete("{id}")]
    [AuthorizeRole("Admin")]
    public async Task<ActionResult> DeletePatient(int id)
    {
        try
        {
            await patientService.DeletePatientAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            logger.LogWarning(e, "Patient {Id} not found when deleting.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to delete patient {Id}.", id);
            return Problem(
                detail: $"Failed to delete patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not delete patient.");
        }
    }
}
