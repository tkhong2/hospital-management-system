using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Inventory.Queries;

public record GetRealtimeInventoryQuery(
    Guid? WarehouseId = null,
    string? Search = null,
    bool? OnlyUnderMinStock = false
) : IRequest<Result<List<RealtimeStockDto>>>;

public class GetRealtimeInventoryQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetRealtimeInventoryQuery, Result<List<RealtimeStockDto>>>
{
    public async Task<Result<List<RealtimeStockDto>>> Handle(GetRealtimeInventoryQuery request, CancellationToken cancellationToken)
    {
        var batchesQuery = dbContext.InventoryBatches
            .AsNoTracking()
            .Include(x => x.Medicine)
            .Include(x => x.Warehouse)
            .Where(x => x.QuantityOnHand > 0);

        if (request.WarehouseId.HasValue)
        {
            batchesQuery = batchesQuery.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            batchesQuery = batchesQuery.Where(x =>
                x.Medicine.Code.ToLower().Contains(search) ||
                x.Medicine.Name.ToLower().Contains(search) ||
                x.Medicine.ActiveIngredient.ToLower().Contains(search) ||
                x.BatchNumber.ToLower().Contains(search));
        }

        var allBatches = await batchesQuery.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var grouped = allBatches
            .GroupBy(b => b.MedicineId)
            .Select(g =>
            {
                var med = g.First().Medicine;
                var totalOnHand = g.Sum(x => x.QuantityOnHand);
                var totalReserved = g.Sum(x => x.QuantityReserved);
                var totalAvailable = totalOnHand - totalReserved;
                var isUnder = totalOnHand <= med.MinStockLevel;

                var batchDtos = g.OrderBy(x => x.ExpiryDate).Select(x =>
                {
                    var days = (int)(x.ExpiryDate.Date - now.Date).TotalDays;
                    return new InventoryBatchDto(
                        x.Id,
                        x.WarehouseId,
                        x.Warehouse.Name,
                        x.MedicineId,
                        med.Code,
                        med.Name,
                        med.UnitName,
                        x.BatchNumber,
                        x.ExpiryDate,
                        x.ManufactureDate,
                        x.InboundPrice,
                        x.SalePrice,
                        x.QuantityOnHand,
                        x.QuantityReserved,
                        x.AvailableQuantity,
                        days,
                        days is > 0 and <= 90,
                        days <= 0
                    );
                }).ToList();

                return new RealtimeStockDto(
                    med.Id,
                    med.Code,
                    med.Name,
                    med.ActiveIngredient,
                    med.Concentration,
                    med.UnitName,
                    med.SpecialControl,
                    totalOnHand,
                    totalReserved,
                    totalAvailable,
                    med.MinStockLevel,
                    isUnder,
                    batchDtos
                );
            })
            .ToList();

        if (request.OnlyUnderMinStock == true)
        {
            grouped = grouped.Where(x => x.IsUnderMinStock).ToList();
        }

        return Result.Success(grouped.OrderBy(x => x.MedicineName).ToList());
    }
}

