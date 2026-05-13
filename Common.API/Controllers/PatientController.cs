using System.Net;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Common.Data.Entity;
using Common.Data.Integration;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientController> _logger;

    public PatientController(IPatientService patientService, ILogger<PatientController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetById(int id)
    {
        try
        {
            Patient? patient = await _patientService.GetByIdAsync(id);
            if (patient is null)
                return NotFound();

            return Ok(patient);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch patient with id {Id}.", id);
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
            Patient patient = await _patientService.GetPatientDetailsAsync(id);
            return Ok(patient);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Patient with id {Id} not found.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch details for patient {Id}.", id);
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
            MedicalHistory? history = await _patientService.GetMedicalHistoryAsync(id);
            if (history is null)
                return NotFound();

            return Ok(history);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Invalid patient id {Id} when fetching medical history.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch medical history for patient {Id}.", id);
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
            List<MedicalRecord> records = await _patientService.GetMedicalRecordsAsync(id);
            return Ok(records);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch medical records for history {Id}.", id);
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
            List<string> allergies = await _patientService.GetPatientAllergiesAsync(id);
            return Ok(allergies);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch allergies for patient {Id}.", id);
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
            bool isHighRisk = await _patientService.IsHighRiskPatientAsync(id);
            return Ok(isHighRisk);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to evaluate high-risk status for patient {Id}.", id);
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
            bool exists = await _patientService.ExistsAsync(cnp);
            return Ok(exists);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to check existence for CNP {Cnp}.", cnp);
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
            Prescription? prescription = await _patientService.GetPrescriptionByRecordIdAsync(recordId);
            if (prescription is null)
                return NotFound();

            return Ok(prescription);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "Prescription repository unavailable.");
            return Problem(
                detail: e.Message,
                statusCode: (int)HttpStatusCode.ServiceUnavailable,
                title: "Prescription service unavailable.");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to fetch prescription for record {RecordId}.", recordId);
            return Problem(
                detail: $"Failed to fetch prescription for record {recordId}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not fetch prescription.");
        }
    }

    [HttpPost]
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

            Patient created = await _patientService.CreatePatientAsync(patient);
            return Ok(created);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Validation failed when creating patient.");
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create patient.");
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

            List<Patient> results = await _patientService.SearchPatientsAsync(filter);
            return Ok(results);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Invalid search filter provided.");
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to search patients.");
            return Problem(
                detail: "Failed to search patients.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not search patients.");
        }
    }

    [HttpPost("{id}/medical-history")]
    public async Task<ActionResult> CreateMedicalHistory(int id, [FromBody] CreateMedicalHistoryDto dto)
    {
        try
        {
            var history = new MedicalHistory
            {
                BloodType = dto.BloodType,
                Rh = dto.Rh,
                ChronicConditions = dto.ChronicConditions,
            };

            await _patientService.CreateMedicalHistoryAsync(id, history);
            return Ok();
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Validation failed when creating medical history for patient {Id}.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create medical history for patient {Id}.", id);
            return Problem(
                detail: $"Failed to create medical history for patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not create medical history.");
        }
    }

    [HttpPut("{id}")]
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
                Dod = dto.Dod,
                Sex = dto.Sex,
                PhoneNo = dto.PhoneNo,
                EmergencyContact = dto.EmergencyContact,
                IsArchived = dto.IsArchived,
                IsDonor = dto.IsDonor,
                Transferred = dto.Transferred,
            };

            await _patientService.UpdatePatientAsync(patient);
            return Ok();
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Validation failed when updating patient {Id}.", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to update patient {Id}.", id);
            return Problem(
                detail: $"Failed to update patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not update patient.");
        }
    }

    [HttpPut("{id}/archive")]
    public async Task<ActionResult> ArchivePatient(int id)
    {
        try
        {
            Patient? patient = await _patientService.GetByIdAsync(id);
            if (patient is null)
                return NotFound();

            await _patientService.ArchivePatientAsync(patient);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to archive patient {Id}.", id);
            return Problem(
                detail: $"Failed to archive patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not archive patient.");
        }
    }

    [HttpPut("{id}/dearchive")]
    public async Task<ActionResult> DearchivePatient(int id)
    {
        try
        {
            await _patientService.DearchivePatientAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Patient {Id} not found when dearchiving.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to dearchive patient {Id}.", id);
            return Problem(
                detail: $"Failed to dearchive patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not dearchive patient.");
        }
    }

    [HttpPut("{id}/archive-deceased")]
    public async Task<ActionResult> ArchiveAsDeceased(int id, [FromBody] ArchiveAsDeceasedDto dto)
    {
        try
        {
            await _patientService.ArchiveAsDeceasedAsync(id, dto.DeathDate);
            return Ok();
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Validation failed when archiving patient {Id} as deceased.", id);
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Patient {Id} not found when archiving as deceased.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to archive patient {Id} as deceased.", id);
            return Problem(
                detail: $"Failed to archive patient {id} as deceased.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not archive patient as deceased.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePatient(int id)
    {
        try
        {
            await _patientService.DeletePatientAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Patient {Id} not found when deleting.", id);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to delete patient {Id}.", id);
            return Problem(
                detail: $"Failed to delete patient {id}.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not delete patient.");
        }
    }
}
