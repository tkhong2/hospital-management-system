using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using HospitalMS.Pharmacy.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Prescriptions.Commands;

public record DispensePrescriptionCommand(
    Guid PrescriptionId,
    Guid? WarehouseId = null,
    string? DispensedBy = "Dược sĩ phụ trách"
) : IRequest<Result<bool>>;

public class DispensePrescriptionCommandHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<DispensePrescriptionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DispensePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.PrescriptionOrders
            .Include(x => x.Details)
                .ThenInclude(d => d.Medicine)
            .FirstOrDefaultAsync(x => x.Id == request.PrescriptionId, cancellationToken);

        if (order == null)
        {
            return Result.Failure<bool>("Không tìm thấy đơn thuốc cần cấp phát");
        }

        if (order.Status == PrescriptionStatus.Dispensed)
        {
            return Result.Failure<bool>("Đơn thuốc này đã được cấp phát hoàn tất trước đó");
        }

        if (order.Status == PrescriptionStatus.Cancelled)
        {
            return Result.Failure<bool>("Đơn thuốc này đã bị hủy, không thể cấp phát");
        }

        var warehouseId = request.WarehouseId ?? order.WarehouseId;
        var now = DateTime.UtcNow;

        // Bắt đầu Transaction đảm bảo tính toàn vẹn cấp phát và trừ kho
        await using var transaction = await dbContext.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var detail in order.Details)
            {
                // Tìm các lô thuốc trong kho cấp phát
                var batches = await dbContext.InventoryBatches
                    .Where(b => b.WarehouseId == warehouseId && b.MedicineId == detail.MedicineId)
                    .ToListAsync(cancellationToken);

                // Tự động phân bổ lô theo FEFO (Hạn dùng gần nhất xuất trước)
                var allocations = FefoAllocationService.Allocate(batches, detail.QuantityRequested, now);

                // Cập nhật từng lô được phân bổ
                foreach (var alloc in allocations)
                {
                    var targetBatch = batches.First(b => b.Id == alloc.BatchId);
                    var balanceBefore = targetBatch.QuantityOnHand;

                    targetBatch.DeductStock(alloc.AllocatedQuantity);

                    // Cập nhật thông tin lô cấp phát vào chi tiết đơn
                    detail.AllocatedBatchId = targetBatch.Id;
                    detail.AllocatedBatchNumber = targetBatch.BatchNumber;
                    detail.AllocatedExpiryDate = targetBatch.ExpiryDate;
                    detail.QuantityDispensed += alloc.AllocatedQuantity;
                    detail.UnitPrice = alloc.UnitPrice;

                    // Ghi sổ thẻ kho (StockLedger)
                    dbContext.StockLedgers.Add(new StockLedger
                    {
                        LedgerCode = $"TK-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
                        WarehouseId = warehouseId,
                        MedicineId = detail.MedicineId,
                        BatchId = targetBatch.Id,
                        BatchNumber = targetBatch.BatchNumber,
                        ExpiryDate = targetBatch.ExpiryDate,
                        TransactionType = LedgerTransactionType.PrescriptionDispense,
                        DocumentNumber = order.PrescriptionCode,
                        Quantity = -alloc.AllocatedQuantity, // Giảm tồn kho (-)
                        BalanceBefore = balanceBefore,
                        BalanceAfter = targetBatch.QuantityOnHand,
                        UnitPrice = alloc.UnitPrice,
                        TransactionDate = now,
                        CreatedBy = request.DispensedBy,
                        Note = $"Cấp phát đơn thuốc cho BN {order.PatientName} ({order.PatientCode})"
                    });
                }
            }

            order.Status = PrescriptionStatus.Dispensed;
            order.DispensedBy = request.DispensedBy;
            order.DispensedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<bool>("Xung đột cấp phát (Concurrency Conflict): Lô thuốc vừa bị quầy khác trừ tồn. Hệ thống đã hủy giao dịch để bảo toàn kho, vui lòng tải lại đơn thuốc!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<bool>($"Lỗi khi cấp phát đơn thuốc: {ex.Message}");
        }
    }
}

