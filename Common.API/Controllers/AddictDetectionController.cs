using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/addicts")]
[AuthorizeRole("Admin", "Medic")]
public class AddictDetectionController : ControllerBase
{
    private readonly IAddictDetectionService addictDetectionService;

    public AddictDetectionController(IAddictDetectionService addictDetectionService)
    {
        this.addictDetectionService = addictDetectionService;
    }

    [HttpGet("candidates")]
    public async Task<ActionResult<List<Patient>>> GetAddictCandidatesAsync()
    {
        try
        {
            List<Patient> candidates = await addictDetectionService.GetAddictCandidatesAsync();
            return Ok(candidates);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not get addict candidates.");
        }
    }

    [HttpPost("police-report")]
    public async Task<ActionResult<string>> BuildPoliceReportAsync([FromBody] BuildPoliceReportRequestDto dto)
    {
        try
        {
            string report = await addictDetectionService.BuildPoliceReportAsync(dto.PatientId);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.BadRequest,
                title: "Invalid patient data.");
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not build police report.");
        }
    }

    [HttpPost("{patientId:int}/notify")]
    public async Task<ActionResult> MarkPoliceNotifiedAsync([FromRoute] int patientId)
    {
        try
        {
            await addictDetectionService.MarkPoliceNotifiedAsync(patientId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.BadRequest,
                title: "Invalid patient ID.");
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not mark patient as police notified.");
        }
    }

    [HttpGet("{patientId:int}/chronic-conditions")]
    public async Task<ActionResult<string>> GetChronicConditionsAsync([FromRoute] int patientId)
    {
        try
        {
            string chronicConditions = await addictDetectionService.GetChronicConditionsAsync(patientId);
            return Ok(chronicConditions);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.BadRequest,
                title: "Invalid patient id.");
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not get chronic conditions.");
        }
    }
}
