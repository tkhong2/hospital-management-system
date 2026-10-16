using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Application.Features.Clinical;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Inpatient.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClinicalController(ISender sender) : ControllerBase
{
    [HttpPost("orders")]
    public async Task<ActionResult<ApiResponse<MedicalOrderDto>>> CreateOrder([FromBody] CreateMedicalOrderCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<MedicalOrderDto>.Fail(result.Error!));

        return Ok(ApiResponse<MedicalOrderDto>.Ok(result.Value!));
    }

    [HttpPost("vitals")]
    public async Task<ActionResult<ApiResponse<VitalSignDto>>> RecordVital([FromBody] RecordVitalSignCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<VitalSignDto>.Fail(result.Error!));

        return Ok(ApiResponse<VitalSignDto>.Ok(result.Value!));
    }

    [HttpPost("care-notes")]
    public async Task<ActionResult<ApiResponse<CareNoteDto>>> CreateCareNote([FromBody] CreateCareNoteCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<CareNoteDto>.Fail(result.Error!));

        return Ok(ApiResponse<CareNoteDto>.Ok(result.Value!));
    }

    [HttpPost("advances")]
    public async Task<ActionResult<ApiResponse<AdvancePaymentDto>>> RecordAdvance([FromBody] RecordAdvancePaymentCommand command)
    {
        var result = await sender.Send(command);
        if (result.IsFailure)
            return BadRequest(ApiResponse<AdvancePaymentDto>.Fail(result.Error!));

        return Ok(ApiResponse<AdvancePaymentDto>.Ok(result.Value!));
    }
}

