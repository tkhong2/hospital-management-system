using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.Medicines.Commands;
using HospitalMS.Pharmacy.Application.Features.Medicines.Queries;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class MedicinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicineDto>>>> GetMedicines(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] MedicineType? medicineType = null,
        [FromQuery] SpecialControlType? specialControl = null,
        [FromQuery] bool? isActive = true)
    {
        var result = await mediator.Send(new GetMedicinesQuery(
            pageNumber, pageSize, search, medicineType, specialControl, isActive));

        return Ok(ApiResponse<PagedResult<MedicineDto>>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMedicine([FromBody] CreateMedicineCommand command)
    {
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<Guid>.Fail(result.Error!));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value!));
    }
}

