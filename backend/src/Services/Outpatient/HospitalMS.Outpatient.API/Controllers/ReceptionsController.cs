using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Application.Features.Receptions;
using HospitalMS.Outpatient.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Outpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReceptionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReceptionDto>>> Create([FromBody] CreateReceptionCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<ReceptionDto>.Fail(result.Error!));

        return Ok(ApiResponse<ReceptionDto>.Ok(result.Value!));
    }

    [HttpGet("queue")]
    public async Task<ActionResult<ApiResponse<List<ReceptionDto>>>> GetQueue(
        [FromQuery] Guid? clinicRoomId,
        [FromQuery] ReceptionStatus? status)
    {
        var result = await sender.Send(new GetReceptionQueueQuery(clinicRoomId, status));
        return Ok(ApiResponse<List<ReceptionDto>>.Ok(result.Value!));
    }
}

