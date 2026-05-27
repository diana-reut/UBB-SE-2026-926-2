using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/examinations")]
    [AuthorizeRole("Admin", "Medic")]
    public class ExaminationController : ControllerBase
    {
        private readonly IExaminationService examinationService;
        private readonly ILogger<ExaminationController> logger;

        public ExaminationController(IExaminationService examinationService, ILogger<ExaminationController> logger)
        {
            this.examinationService = examinationService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Examination>>> GetAll()
        {
            try
            {
                var result = await examinationService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch examinations.");

                return Problem(
                    detail: "Failed to fetch examinations.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examinations.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Examination>> GetById(int id)
        {
            try
            {
                Examination? result = await examinationService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("Examination {ExamId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch examination {ExamId}.", id);

                return Problem(
                    detail: "Failed to fetch examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examination.");
            }
        }

        [HttpGet("visit/{visitId:int}")]
        public async Task<ActionResult<List<Examination>>> GetByVisitId(int visitId)
        {
            try
            {
                return Ok(await examinationService.GetByVisitIdAsync(visitId));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch examinations for visit {VisitId}.", visitId);
                return Problem(
                    detail: "Failed to fetch examinations for visit.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examinations.");
            }
        }

        [HttpGet("eligible-visits")]
        public async Task<ActionResult<List<ER_Visit>>> GetEligibleVisits()
        {
            try
            {
                return Ok(await examinationService.GetEligibleVisitsAsync());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch eligible examination visits.");
                return Problem(
                    detail: "Failed to fetch eligible examination visits.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch eligible examination visits.");
            }
        }

        [HttpGet("patient-history/{patientId}")]
        public async Task<ActionResult<List<Examination>>> GetPatientHistory(string patientId)
        {
            try
            {
                return Ok(await examinationService.GetPatientHistoryAsync(patientId));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch examination history for patient {PatientId}.", patientId);
                return Problem(
                    detail: "Failed to fetch examination history.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examination history.");
            }
        }

        [HttpGet("summary/{visitId:int}")]
        public async Task<ActionResult<ERExaminationSummaryDto>> GetSummary(int visitId)
        {
            try
            {
                ERExaminationSummaryDto? result = await examinationService.GetSummaryByVisitIdAsync(visitId);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch examination summary for visit {VisitId}.", visitId);
                return Problem(
                    detail: "Failed to fetch examination summary.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examination summary.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Examination>> Create([FromBody] Examination examination)
        {
            try
            {
                Examination result = await examinationService.CreateAsync(examination);
                return CreatedAtAction(nameof(GetById), new { id = result.Exam_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create examination.");

                return Problem(
                    detail: "Failed to create examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create examination.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Examination examination)
        {
            try
            {
                bool updated = await examinationService.UpdateAsync(id, examination);
                if (!updated)
                {
                    logger.LogWarning("Examination {ExamId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update examination {ExamId}.", id);

                return Problem(
                    detail: "Failed to update examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update examination.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await examinationService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("Examination {ExamId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete examination {ExamId}.", id);

                return Problem(
                    detail: "Failed to delete examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete examination.");
            }
        }
    }
}
