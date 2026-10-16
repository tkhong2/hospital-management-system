using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Application.Features.Encounters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Outpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EncountersController(ISender sender) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<EncounterDto>>> Start([FromBody] StartEncounterCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<EncounterDto>.Fail(result.Error!));

        return Ok(ApiResponse<EncounterDto>.Ok(result.Value!));
    }

    [HttpPut("{id:guid}/diagnosis")]
    public async Task<ActionResult<ApiResponse<EncounterDto>>> UpdateDiagnosis(
        Guid id,
        [FromBody] UpdateEncounterDiagnosisCommand command)
    {
        if (id != command.EncounterId)
            return BadRequest(ApiResponse<EncounterDto>.Fail("Id phiên khám không khớp"));

        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<EncounterDto>.Fail(result.Error!));

        return Ok(ApiResponse<EncounterDto>.Ok(result.Value!));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<bool>>> Complete(Guid id)
    {
        var result = await sender.Send(new CompleteEncounterCommand(id));
        if (result.IsFailure)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EncounterDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetEncounterDetailQuery(id));
        if (result.IsFailure)
            return NotFound(ApiResponse<EncounterDto>.Fail(result.Error!));

        return Ok(ApiResponse<EncounterDto>.Ok(result.Value!));
    }
}

