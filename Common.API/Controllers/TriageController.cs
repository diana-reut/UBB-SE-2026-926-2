using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Data;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/triages")]
    [AuthorizeRole("Admin", "Medic")]
    public class TriageController : ControllerBase
    {
        private readonly ITriageService triageService;
        private readonly ITriageDecisionService triageDecisionService;
        private readonly EFHospitalDbContext dbContext;
        private readonly ILogger<TriageController> logger;

        public TriageController(
            ITriageService triageService,
            ITriageDecisionService triageDecisionService,
            EFHospitalDbContext dbContext,
            ILogger<TriageController> logger)
        {
            this.triageService = triageService;
            this.triageDecisionService = triageDecisionService;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Triage>>> GetAll()
        {
            try
            {
                var result = await triageService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch triages.");

                return Problem(
                    detail: "Failed to fetch triages.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triages.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Triage>> GetById(int id)
        {
            try
            {
                Triage? result = await triageService.GetByIdAsync(id);
                if (result is null)
                {
                    logger.LogWarning("Triage {TriageId} was not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to fetch triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch triage.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Triage>> Create([FromBody] Triage triage)
        {
            try
            {
                Triage result = await triageService.CreateAsync(triage);
                return CreatedAtAction(nameof(GetById), new { id = result.Triage_ID }, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create triage.");

                return Problem(
                    detail: "Failed to create triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not create triage.");
            }
        }

        [HttpPost("perform")]
        public async Task<ActionResult<PerformTriageResponseDto>> Perform([FromBody] PerformTriageRequestDto request)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                ER_Visit? visit = await dbContext.ERVisits.FirstOrDefaultAsync(item => item.Visit_ID == request.VisitId);
                if (visit is null)
                {
                    return NotFound($"Visit {request.VisitId} was not found.");
                }

                Triage_Parameters pendingParameters = request.ToParameters(triageId: 0);
                pendingParameters.ValidateParameters();
                int triageLevel = triageDecisionService.CalculateTriageLevel(pendingParameters);
                string specialization = triageDecisionService.DetermineSpecialization(pendingParameters);

                Triage? triage = await dbContext.Triages.FirstOrDefaultAsync(item => item.Visit_ID == request.VisitId);
                if (triage is not null)
                {
                    Triage_Parameters? existingParameters = await dbContext.TriageParameters
                        .FirstOrDefaultAsync(item => item.TriageId == triage.Triage_ID);

                    if (existingParameters is not null)
                    {
                        return Conflict($"Triage has already been completed for visit {request.VisitId}.");
                    }

                    triage.Triage_Level = triageLevel;
                    triage.Specialization = specialization;
                    triage.Nurse_ID = request.NurseId;
                    triage.Triage_Time = request.TriageTime;
                }
                else
                {
                    triage = new Triage
                    {
                        Visit_ID = request.VisitId,
                        Triage_Level = triageLevel,
                        Specialization = specialization,
                        Nurse_ID = request.NurseId,
                        Triage_Time = request.TriageTime
                    };

                    await dbContext.Triages.AddAsync(triage);
                    await dbContext.SaveChangesAsync();
                }

                Triage_Parameters parameters = request.ToParameters(triage.Triage_ID);
                await dbContext.TriageParameters.AddAsync(parameters);

                visit.Status = ER_Visit.VisitStatus.TRIAGED;
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new PerformTriageResponseDto
                {
                    Triage = triage,
                    Parameters = parameters
                });
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                logger.LogError(e, "Failed to perform triage for visit {VisitId}.", request.VisitId);

                return Problem(
                    detail: e.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not perform triage.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Triage triage)
        {
            try
            {
                bool updated = await triageService.UpdateAsync(id, triage);
                if (!updated)
                {
                    logger.LogWarning("Triage {TriageId} was not found for update.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to update triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not update triage.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await triageService.DeleteAsync(id);
                if (!deleted)
                {
                    logger.LogWarning("Triage {TriageId} was not found for delete.", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete triage {TriageId}.", id);

                return Problem(
                    detail: "Failed to delete triage.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not delete triage.");
            }
        }
    }
}
