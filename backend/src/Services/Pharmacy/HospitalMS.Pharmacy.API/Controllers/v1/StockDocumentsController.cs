using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Application.Features.StockDocuments.Commands;
using HospitalMS.Pharmacy.Application.Features.StockDocuments.Queries;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Pharmacy.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class StockDocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StockDocumentDto>>>> GetDocuments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DocumentType? documentType = null,
        [FromQuery] DocumentStatus? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetStockDocumentsQuery(
            pageNumber, pageSize, documentType, status, warehouseId, search));

        return Ok(ApiResponse<PagedResult<StockDocumentDto>>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateDocument([FromBody] CreateStockDocumentCommand command)
    {
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<Guid>.Fail(result.Error!));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value!));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveDocument(
        Guid id,
        [FromQuery] string? approvedBy = "Dược sĩ phụ trách")
    {
        var result = await mediator.Send(new ApproveStockDocumentCommand(id, approvedBy));
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }
}

