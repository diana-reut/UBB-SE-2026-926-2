using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Common.API.Controllers;

[ApiController]
[Route("api/addicts")]
[AuthorizeRole("Admin", "Medic")]
public class AddictDetectionController : ControllerBase
{
    private readonly IAddictDetectionService _addictDetectionService;

    public AddictDetectionController(IAddictDetectionService addictDetectionService)
    {
        _addictDetectionService = addictDetectionService;
    }

    [HttpGet("candidates")]
    public async Task<ActionResult<List<Patient>>> GetAddictCandidatesAsync()
    {
        try
        {
            List<Patient> candidates = await _addictDetectionService.GetAddictCandidatesAsync();
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
            string report = await _addictDetectionService.BuildPoliceReportAsync(dto.PatientId);
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
            await _addictDetectionService.MarkPoliceNotifiedAsync(patientId);
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
            string chronicConditions = await _addictDetectionService.GetChronicConditionsAsync(patientId);
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
