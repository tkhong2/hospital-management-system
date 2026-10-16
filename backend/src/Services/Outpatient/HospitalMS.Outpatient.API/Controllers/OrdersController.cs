using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Application.Features.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Outpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost("services")]
    public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> CreateServiceOrder([FromBody] CreateServiceOrderCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<ServiceOrderDto>.Fail(result.Error!));

        return Ok(ApiResponse<ServiceOrderDto>.Ok(result.Value!));
    }

    [HttpPost("prescriptions")]
    public async Task<ActionResult<ApiResponse<PrescriptionDto>>> CreatePrescription([FromBody] CreatePrescriptionCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail(result.Error!));

        return Ok(ApiResponse<PrescriptionDto>.Ok(result.Value!));
    }

    [HttpPost("billings")]
    public async Task<ActionResult<ApiResponse<BillingDto>>> ProcessBilling([FromBody] ProcessOutpatientBillingCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<BillingDto>.Fail(result.Error!));

        return Ok(ApiResponse<BillingDto>.Ok(result.Value!));
    }
}

