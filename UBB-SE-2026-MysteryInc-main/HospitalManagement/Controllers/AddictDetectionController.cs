using Common.Data.Entity;
using HospitalManagement.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Controller;

[ApiController]
[Route("api/addicts")]
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
                statusCode: 500,
                title: "Could not get addict candidates."
            );
        }
    }


    [HttpPost("police-report")]
    public async Task<ActionResult<string>> BuildPoliceReportAsync([FromBody] BuildPoliceReportRequestDto dto)
    {
        try
        {
            string report = await _addictDetectionService.BuildPoliceReportAsync(dto.Patient);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 400,
                title: "Invalid patient data."
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Could not build police report."
            );
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
                statusCode: 400,
                title: "Invalid patient id."
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Could not get chronic conditions."
            );
        }
    }
}