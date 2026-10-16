using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("realtime")]
    public async Task<ActionResult<ApiResponse<List<RealtimeStockDto>>>> GetRealtimeInventory(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? onlyUnderMinStock = false)
    {
        var result = await mediator.Send(new GetRealtimeInventoryQuery(warehouseId, search, onlyUnderMinStock));
        return Ok(ApiResponse<List<RealtimeStockDto>>.Ok(result.Value!));
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<ApiResponse<List<StockAlertDto>>>> GetStockAlerts(
        [FromQuery] Guid? warehouseId = null)
    {
        var result = await mediator.Send(new GetStockAlertsQuery(warehouseId));
        return Ok(ApiResponse<List<StockAlertDto>>.Ok(result.Value!));
    }
}

