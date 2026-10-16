using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("stock-ledger")]
    public async Task<ActionResult<ApiResponse<List<StockReportItemDto>>>> GetStockReport(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetStockReportQuery(warehouseId, fromDate, toDate, search));
        return Ok(ApiResponse<List<StockReportItemDto>>.Ok(result.Value!));
    }
}

