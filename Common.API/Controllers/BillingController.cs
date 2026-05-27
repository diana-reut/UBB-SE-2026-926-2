using System;
using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers;

[ApiController]
[Route("api/billing")]
[AuthorizeRole("Admin", "Medic")]
public class BillingController : ControllerBase
{
    private readonly IBillingService billingService;

    public BillingController(IBillingService billingService)
    {
        this.billingService = billingService;
    }

    [HttpGet("base-price/{patientId:int}/{recordId:int}")]
    public async Task<ActionResult<decimal>> ComputeBasePriceAsync(
        [FromRoute] int patientId,
        [FromRoute] int recordId)
    {
        try
        {
            decimal price = await billingService.ComputeBasePriceAsync(patientId, recordId);
            return Ok(price);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not compute base price.");
        }
    }

    [HttpPost("discount")]
    public async Task<ActionResult<decimal>> ApplyDiscountAsync([FromBody] ApplyDiscountRequestDto dto)
    {
        try
        {
            decimal finalPrice = await billingService.ApplyDiscountAsync(dto.BasePrice, dto.Discount);
            return Ok(finalPrice);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not apply discount.");
        }
    }

    [HttpPost("discount/{recordId:int}")]
    public async Task<ActionResult<decimal>> PersistDiscountAsync(
        [FromRoute] int recordId,
        [FromBody] ApplyDiscountRequestDto dto)
    {
        try
        {
            decimal finalPrice = await billingService.PersistDiscountAsync(recordId, dto.BasePrice, dto.Discount);
            return Ok(finalPrice);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.NotFound,
                title: "Medical record not found.");
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Could not persist discount.");
        }
    }
}
