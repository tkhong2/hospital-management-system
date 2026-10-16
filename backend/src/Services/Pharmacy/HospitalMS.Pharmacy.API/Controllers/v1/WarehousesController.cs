using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.Warehouses.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class WarehousesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseDto>>>> GetWarehouses([FromQuery] bool? isActive = true)
    {
        var result = await mediator.Send(new GetWarehousesQuery(isActive));
        return Ok(ApiResponse<List<WarehouseDto>>.Ok(result.Value!));
    }
}

