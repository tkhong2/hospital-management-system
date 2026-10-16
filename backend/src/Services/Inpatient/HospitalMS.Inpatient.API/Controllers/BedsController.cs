using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Application.Features.Beds;
using HospitalMS.Inpatient.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Inpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BedsController(ISender sender) : ControllerBase
{
    [HttpGet("map")]
    public async Task<ActionResult<ApiResponse<BedMapSummaryDto>>> GetBedMap([FromQuery] string? departmentCode)
    {
        var result = await sender.Send(new GetBedMapQuery(departmentCode));
        return Ok(ApiResponse<BedMapSummaryDto>.Ok(result.Value!));
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<bool>>> TransferBed([FromBody] TransferBedCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(
        Guid id,
        [FromQuery] BedStatus newStatus,
        [FromQuery] string? notes)
    {
        var result = await sender.Send(new UpdateBedStatusCommand(id, newStatus, notes));
        if (result.IsFailure)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }
}

