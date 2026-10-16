using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Reports.Queries;

public record GetStockReportQuery(
    Guid? WarehouseId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null
) : IRequest<Result<List<StockReportItemDto>>>;

public class GetStockReportQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetStockReportQuery, Result<List<StockReportItemDto>>>
{
    public async Task<Result<List<StockReportItemDto>>> Handle(GetStockReportQuery request, CancellationToken cancellationToken)
    {
        var from = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = request.ToDate ?? DateTime.UtcNow;

        var medicinesQuery = dbContext.Medicines.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            medicinesQuery = medicinesQuery.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search));
        }

        var medicines = await medicinesQuery.ToListAsync(cancellationToken);

        var ledgerQuery = dbContext.StockLedgers.AsNoTracking().AsQueryable();
        if (request.WarehouseId.HasValue)
        {
            ledgerQuery = ledgerQuery.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        var allLedgers = await ledgerQuery.ToListAsync(cancellationToken);

        var reports = new List<StockReportItemDto>();

        foreach (var med in medicines)
        {
            var medLedgers = allLedgers.Where(x => x.MedicineId == med.Id).ToList();

            // Tồn đầu kỳ (trước FromDate)
            var openingBalance = medLedgers
                .Where(x => x.TransactionDate < from)
                .Sum(x => x.Quantity);

            // Giao dịch trong kỳ [FromDate, ToDate]
            var periodLedgers = medLedgers
                .Where(x => x.TransactionDate >= from && x.TransactionDate <= to)
                .ToList();

            var inboundQty = periodLedgers.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
            var outboundQty = Math.Abs(periodLedgers.Where(x => x.Quantity < 0).Sum(x => x.Quantity));
            var closingBalance = openingBalance + inboundQty - outboundQty;

            // Nếu thuốc có tồn hoặc có phát sinh trong kỳ thì đưa vào báo cáo
            if (openingBalance != 0 || inboundQty != 0 || outboundQty != 0 || closingBalance != 0)
            {
                reports.Add(new StockReportItemDto(
                    MedicineId: med.Id,
                    MedicineCode: med.Code,
                    MedicineName: med.Name,
                    UnitName: med.UnitName,
                    OpeningBalance: openingBalance,
                    InboundQuantity: inboundQty,
                    OutboundQuantity: outboundQty,
                    ClosingBalance: closingBalance,
                    UnitPrice: med.InboundPrice > 0 ? med.InboundPrice : med.SalePrice,
                    ClosingAmount: closingBalance * (med.InboundPrice > 0 ? med.InboundPrice : med.SalePrice)
                ));
            }
        }

        return Result.Success(reports.OrderBy(x => x.MedicineName).ToList());
    }
}

