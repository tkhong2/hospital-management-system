using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Inventory.Queries;

public record GetStockAlertsQuery(Guid? WarehouseId = null)
    : IRequest<Result<List<StockAlertDto>>>;

public class GetStockAlertsQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetStockAlertsQuery, Result<List<StockAlertDto>>>
{
    public async Task<Result<List<StockAlertDto>>> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = new List<StockAlertDto>();
        var now = DateTime.UtcNow.Date;

        var batchesQuery = dbContext.InventoryBatches
            .AsNoTracking()
            .Include(x => x.Medicine)
            .Include(x => x.Warehouse)
            .Where(x => x.QuantityOnHand > 0);

        if (request.WarehouseId.HasValue)
        {
            batchesQuery = batchesQuery.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        var batches = await batchesQuery.ToListAsync(cancellationToken);

        // 1. Cảnh báo hết hạn (Outdated)
        foreach (var b in batches.Where(x => x.ExpiryDate.Date < now))
        {
            alerts.Add(new StockAlertDto(
                AlertType: "Outdated",
                Severity: "Critical",
                MedicineId: b.MedicineId,
                MedicineCode: b.Medicine.Code,
                MedicineName: b.Medicine.Name,
                WarehouseId: b.WarehouseId,
                WarehouseName: b.Warehouse.Name,
                BatchNumber: b.BatchNumber,
                ExpiryDate: b.ExpiryDate,
                CurrentQuantity: b.QuantityOnHand,
                ThresholdQuantity: null,
                Message: $"Lô {b.BatchNumber} đã hết hạn sử dụng vào ngày {b.ExpiryDate:dd/MM/yyyy}. Cần lập biên bản xuất hủy ngay!"
            ));
        }

        // 2. Cảnh báo cận hạn (NearExpiry <= 90 ngày)
        var nearDate = now.AddDays(90);
        foreach (var b in batches.Where(x => x.ExpiryDate.Date >= now && x.ExpiryDate.Date <= nearDate))
        {
            var daysRemaining = (int)(b.ExpiryDate.Date - now).TotalDays;
            alerts.Add(new StockAlertDto(
                AlertType: "NearExpiry",
                Severity: daysRemaining <= 30 ? "Critical" : "Warning",
                MedicineId: b.MedicineId,
                MedicineCode: b.Medicine.Code,
                MedicineName: b.Medicine.Name,
                WarehouseId: b.WarehouseId,
                WarehouseName: b.Warehouse.Name,
                BatchNumber: b.BatchNumber,
                ExpiryDate: b.ExpiryDate,
                CurrentQuantity: b.QuantityOnHand,
                ThresholdQuantity: null,
                Message: $"Lô {b.BatchNumber} cận hạn dùng ({daysRemaining} ngày còn lại, HSD: {b.ExpiryDate:dd/MM/yyyy}). Ưu tiên xuất theo FEFO!"
            ));
        }

        // 3. Cảnh báo tồn dưới mức tối thiểu (UnderMinStock)
        var groupedMedicines = batches
            .GroupBy(x => new { x.MedicineId, x.WarehouseId })
            .Select(g => new
            {
                g.Key.MedicineId,
                g.Key.WarehouseId,
                FirstBatch = g.First(),
                TotalOnHand = g.Sum(x => x.QuantityOnHand)
            });

        foreach (var g in groupedMedicines)
        {
            var med = g.FirstBatch.Medicine;
            if (g.TotalOnHand <= med.MinStockLevel)
            {
                alerts.Add(new StockAlertDto(
                    AlertType: "UnderMinStock",
                    Severity: g.TotalOnHand == 0 ? "Critical" : "Warning",
                    MedicineId: med.Id,
                    MedicineCode: med.Code,
                    MedicineName: med.Name,
                    WarehouseId: g.WarehouseId,
                    WarehouseName: g.FirstBatch.Warehouse.Name,
                    BatchNumber: null,
                    ExpiryDate: null,
                    CurrentQuantity: g.TotalOnHand,
                    ThresholdQuantity: med.MinStockLevel,
                    Message: $"Thuốc/vật tư '{med.Name}' tồn kho ({g.TotalOnHand:N0}) chạm/dưới định mức tối thiểu ({med.MinStockLevel:N0}). Cần lập dự trù nhập hàng!"
                ));
            }
        }

        return Result.Success(alerts.OrderByDescending(x => x.Severity == "Critical").ThenBy(x => x.MedicineName).ToList());
    }
}

