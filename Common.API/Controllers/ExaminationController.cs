using System.Net;
using Common.API.Services;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/examinations")]
    public class ExaminationController : ControllerBase
    {
        private readonly IExaminationService _examinationService;
        private readonly ILogger<ExaminationController> _logger;

        public ExaminationController(IExaminationService examinationService, ILogger<ExaminationController> logger)
        {
            _examinationService = examinationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Examination>>> GetAll()
        {
            try
            {
                var result = await _examinationService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch examinations.");

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
                Examination? result = await _examinationService.GetByIdAsync(id);
                if (result is null)
                {
                    _logger.LogWarning("Examination {ExamId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch examination {ExamId}.", id);

                return Problem(
                    detail: "Failed to fetch examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch examination.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Examination>> Create([FromBody] Examination examination)
        {
            try
            {
                Examination result = await _examinationService.CreateAsync(examination);
                return CreatedAtAction(nameof(GetById), new { id = result.Exam_ID }, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create examination.");

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
                bool updated = await _examinationService.UpdateAsync(id, examination);
                if (!updated)
                {
                    _logger.LogWarning("Examination {ExamId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update examination {ExamId}.", id);

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
                bool deleted = await _examinationService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Examination {ExamId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete examination {ExamId}.", id);

                return Problem(
                    detail: "Failed to delete examination.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete examination.");
            }
        }
    }
}
