using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.StockDocuments.Queries;

public record GetStockDocumentsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    DocumentType? DocumentType = null,
    DocumentStatus? Status = null,
    Guid? WarehouseId = null,
    string? Search = null
) : IRequest<Result<PagedResult<StockDocumentDto>>>;

public class GetStockDocumentsQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetStockDocumentsQuery, Result<PagedResult<StockDocumentDto>>>
{
    public async Task<Result<PagedResult<StockDocumentDto>>> Handle(GetStockDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.StockDocuments
            .AsNoTracking()
            .Include(x => x.SourceWarehouse)
            .Include(x => x.TargetWarehouse)
            .Include(x => x.Details)
                .ThenInclude(d => d.Medicine)
            .AsQueryable();

        if (request.DocumentType.HasValue)
        {
            query = query.Where(x => x.DocumentType == request.DocumentType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.SourceWarehouseId == request.WarehouseId.Value ||
                x.TargetWarehouseId == request.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.DocumentNumber.ToLower().Contains(search) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var docs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new StockDocumentDto(
                x.Id,
                x.DocumentNumber,
                x.DocumentType,
                GetDocTypeName(x.DocumentType),
                x.SourceWarehouseId,
                x.SourceWarehouse != null ? x.SourceWarehouse.Name : null,
                x.TargetWarehouseId,
                x.TargetWarehouse != null ? x.TargetWarehouse.Name : null,
                x.SupplierName,
                x.DocumentDate,
                x.Status,
                GetStatusName(x.Status),
                x.TotalAmount,
                x.Note,
                x.CreatedBy,
                x.ApprovedBy,
                x.ApprovedAt,
                x.Details.Select(d => new StockDocumentItemDto(
                    d.MedicineId,
                    d.Medicine.Code,
                    d.Medicine.Name,
                    d.SourceBatchId,
                    d.BatchNumber,
                    d.ExpiryDate,
                    d.ManufactureDate,
                    d.Quantity,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.Note
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<StockDocumentDto>(docs, totalCount, request.PageNumber, request.PageSize));
    }

    private static string GetDocTypeName(DocumentType type) => type switch
    {
        DocumentType.Inbound => "Phiếu nhập kho",
        DocumentType.Outbound => "Phiếu xuất kho",
        DocumentType.Transfer => "Phiếu chuyển kho",
        DocumentType.Return => "Phiếu trả hàng",
        DocumentType.Dispose => "Phiếu xuất hủy",
        _ => type.ToString()
    };

    private static string GetStatusName(DocumentStatus status) => status switch
    {
        DocumentStatus.Draft => "Chờ duyệt (Nháp)",
        DocumentStatus.Approved => "Đã duyệt",
        DocumentStatus.Cancelled => "Đã hủy",
        _ => status.ToString()
    };
}

