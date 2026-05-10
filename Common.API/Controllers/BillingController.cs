using Microsoft.AspNetCore.Mvc;
using System;
using Common.Data.Entity.DTOs;
using Common.API.Services;
using System.Net;

namespace Common.API.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("base-price/{patientId:int}/{recordId:int}")]
    public async Task<ActionResult<decimal>> ComputeBasePriceAsync(
        [FromRoute] int patientId,
        [FromRoute] int recordId)
    {
        try
        {
            decimal price = await _billingService.ComputeBasePriceAsync(patientId, recordId);
            return Ok(price);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not compute base price."
            );
        }
    }

    [HttpPost("discount")]
    public async Task<ActionResult<decimal>> ApplyDiscountAsync([FromBody] ApplyDiscountRequestDto dto)
    {
        try
        {
            decimal finalPrice = await _billingService.ApplyDiscountAsync(dto.BasePrice, dto.Discount);
            return Ok(finalPrice);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not apply discount."
            );
        }
    }
}
