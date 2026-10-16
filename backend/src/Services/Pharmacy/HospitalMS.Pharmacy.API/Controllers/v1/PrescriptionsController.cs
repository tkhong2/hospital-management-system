using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.Prescriptions.Commands;
using HospitalMS.Pharmacy.Application.Features.Prescriptions.Queries;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PrescriptionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PrescriptionOrderDto>>>> GetPrescriptionOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] PrescriptionStatus? status = null,
        [FromQuery] PrescriptionSourceType? sourceType = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetPrescriptionOrdersQuery(
            pageNumber, pageSize, status, sourceType, warehouseId, search));

        return Ok(ApiResponse<PagedResult<PrescriptionOrderDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:guid}/dispense")]
    public async Task<ActionResult<ApiResponse<bool>>> DispensePrescription(
        Guid id,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? dispensedBy = "Dược sĩ phụ trách")
    {
        var result = await mediator.Send(new DispensePrescriptionCommand(id, warehouseId, dispensedBy));
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }
}

