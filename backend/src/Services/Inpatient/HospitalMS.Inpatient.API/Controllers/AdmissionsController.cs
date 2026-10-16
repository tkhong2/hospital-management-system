using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Application.Features.Admissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Inpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdmissionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InpatientAdmissionDto>>> Create([FromBody] CreateAdmissionCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<InpatientAdmissionDto>.Fail(result.Error!));

        return Ok(ApiResponse<InpatientAdmissionDto>.Ok(result.Value!));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<InpatientAdmissionDto>>>> GetActive([FromQuery] string? departmentCode)
    {
        var result = await sender.Send(new GetActiveAdmissionsQuery(departmentCode));
        return Ok(ApiResponse<List<InpatientAdmissionDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InpatientAdmissionDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetAdmissionDetailQuery(id));
        if (result.IsFailure)
            return NotFound(ApiResponse<InpatientAdmissionDto>.Fail(result.Error!));

        return Ok(ApiResponse<InpatientAdmissionDto>.Ok(result.Value!));
    }

    [HttpPost("{id:guid}/discharge")]
    public async Task<ActionResult<ApiResponse<bool>>> Discharge(Guid id, [FromBody] DischargeAdmissionCommand command)
    {
        if (id != command.AdmissionId)
            return BadRequest(ApiResponse<bool>.Fail("Id bệnh án không khớp"));

        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }
}

