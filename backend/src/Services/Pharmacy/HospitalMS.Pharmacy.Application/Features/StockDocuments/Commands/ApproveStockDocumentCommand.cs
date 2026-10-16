using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.StockDocuments.Commands;

public record ApproveStockDocumentCommand(Guid DocumentId, string? ApprovedBy = "Dược sĩ phụ trách")
    : IRequest<Result<bool>>;

public class ApproveStockDocumentCommandHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<ApproveStockDocumentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ApproveStockDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await dbContext.StockDocuments
            .Include(x => x.Details)
                .ThenInclude(d => d.Medicine)
            .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);

        if (doc == null)
        {
            return Result.Failure<bool>("Không tìm thấy phiếu kho");
        }

        if (doc.Status == DocumentStatus.Approved)
        {
            return Result.Failure<bool>("Phiếu kho này đã được duyệt trước đó");
        }

        if (doc.Status == DocumentStatus.Cancelled)
        {
            return Result.Failure<bool>("Không thể duyệt phiếu kho đã bị hủy");
        }

        // Open Transaction
        await using var transaction = await dbContext.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            foreach (var detail in doc.Details)
            {
                switch (doc.DocumentType)
                {
                    case DocumentType.Inbound:
                        await HandleInboundAsync(doc, detail, now, cancellationToken);
                        break;

                    case DocumentType.Outbound:
                    case DocumentType.Dispose:
                        await HandleOutboundAsync(doc, detail, now, cancellationToken);
                        break;

                    case DocumentType.Transfer:
                        await HandleTransferAsync(doc, detail, now, cancellationToken);
                        break;

                    default:
                        throw new InvalidOperationException($"Loại phiếu {doc.DocumentType} chưa được hỗ trợ duyệt");
                }
            }

            doc.Status = DocumentStatus.Approved;
            doc.ApprovedBy = request.ApprovedBy;
            doc.ApprovedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<bool>("Xung đột dữ liệu (Concurrency Conflict): Lô thuốc đã bị thay đổi bởi giao dịch khác trong cùng thời điểm. Vui lòng tải lại và thử lại!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<bool>($"Lỗi khi duyệt phiếu kho: {ex.Message}");
        }
    }

    private async Task HandleInboundAsync(
        StockDocument doc,
        StockDocumentDetail detail,
        DateTime now,
        CancellationToken ct)
    {
        var targetWarehouseId = doc.TargetWarehouseId
            ?? throw new InvalidOperationException("Phiếu nhập kho phải có kho nhận hàng (TargetWarehouseId)");

        // Tìm lô trùng số lô và hạn dùng tại kho nhận
        var batch = await dbContext.InventoryBatches
            .FirstOrDefaultAsync(b =>
                b.WarehouseId == targetWarehouseId &&
                b.MedicineId == detail.MedicineId &&
                b.BatchNumber == detail.BatchNumber, ct);

        decimal balanceBefore = 0;

        if (batch != null)
        {
            balanceBefore = batch.QuantityOnHand;
            batch.AddStock(detail.Quantity);
            batch.InboundPrice = detail.UnitPrice;
            batch.ExpiryDate = detail.ExpiryDate;
        }
        else
        {
            batch = new InventoryBatch
            {
                WarehouseId = targetWarehouseId,
                MedicineId = detail.MedicineId,
                BatchNumber = detail.BatchNumber,
                ExpiryDate = detail.ExpiryDate,
                ManufactureDate = detail.ManufactureDate,
                InboundPrice = detail.UnitPrice,
                SalePrice = detail.Medicine.SalePrice > 0 ? detail.Medicine.SalePrice : detail.UnitPrice * 1.15m,
                QuantityOnHand = detail.Quantity,
                QuantityReserved = 0
            };
            dbContext.InventoryBatches.Add(batch);
        }

        // Ghi Sổ thẻ kho (StockLedger)
        dbContext.StockLedgers.Add(new StockLedger
        {
            LedgerCode = $"TK-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
            WarehouseId = targetWarehouseId,
            MedicineId = detail.MedicineId,
            BatchId = batch.Id,
            BatchNumber = detail.BatchNumber,
            ExpiryDate = detail.ExpiryDate,
            TransactionType = LedgerTransactionType.Inbound,
            DocumentNumber = doc.DocumentNumber,
            Quantity = detail.Quantity, // Tăng kho (+)
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceBefore + detail.Quantity,
            UnitPrice = detail.UnitPrice,
            TransactionDate = now,
            CreatedBy = doc.ApprovedBy ?? doc.CreatedBy,
            Note = $"Nhập kho NCC: {doc.SupplierName ?? "N/A"}"
        });
    }

    private async Task HandleOutboundAsync(
        StockDocument doc,
        StockDocumentDetail detail,
        DateTime now,
        CancellationToken ct)
    {
        var sourceWarehouseId = doc.SourceWarehouseId
            ?? throw new InvalidOperationException("Phiếu xuất kho phải có kho xuất (SourceWarehouseId)");

        InventoryBatch? batch = null;
        if (detail.SourceBatchId.HasValue)
        {
            batch = await dbContext.InventoryBatches
                .FirstOrDefaultAsync(b => b.Id == detail.SourceBatchId.Value, ct);
        }

        if (batch == null)
        {
            batch = await dbContext.InventoryBatches
                .Where(b => b.WarehouseId == sourceWarehouseId && b.MedicineId == detail.MedicineId && b.QuantityOnHand >= detail.Quantity)
                .OrderBy(b => b.ExpiryDate)
                .FirstOrDefaultAsync(ct);
        }

        if (batch == null)
        {
            throw new InvalidOperationException(
                $"Kho xuất không có đủ tồn kho cho thuốc '{detail.Medicine.Name}' (Lô: {detail.BatchNumber})");
        }

        var balanceBefore = batch.QuantityOnHand;
        batch.DeductStock(detail.Quantity);

        var ledgerType = doc.DocumentType == DocumentType.Dispose
            ? LedgerTransactionType.Dispose
            : LedgerTransactionType.Outbound;

        dbContext.StockLedgers.Add(new StockLedger
        {
            LedgerCode = $"TK-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
            WarehouseId = sourceWarehouseId,
            MedicineId = detail.MedicineId,
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            ExpiryDate = batch.ExpiryDate,
            TransactionType = ledgerType,
            DocumentNumber = doc.DocumentNumber,
            Quantity = -detail.Quantity, // Giảm kho (-)
            BalanceBefore = balanceBefore,
            BalanceAfter = batch.QuantityOnHand,
            UnitPrice = detail.UnitPrice > 0 ? detail.UnitPrice : batch.SalePrice,
            TransactionDate = now,
            CreatedBy = doc.ApprovedBy ?? doc.CreatedBy,
            Note = doc.Note ?? "Xuất kho dược"
        });
    }

    private async Task HandleTransferAsync(
        StockDocument doc,
        StockDocumentDetail detail,
        DateTime now,
        CancellationToken ct)
    {
        var sourceWarehouseId = doc.SourceWarehouseId
            ?? throw new InvalidOperationException("Chuyển kho thiếu SourceWarehouseId");
        var targetWarehouseId = doc.TargetWarehouseId
            ?? throw new InvalidOperationException("Chuyển kho thiếu TargetWarehouseId");

        // 1. Trừ kho nguồn
        var sourceBatch = await dbContext.InventoryBatches
            .FirstOrDefaultAsync(b =>
                b.WarehouseId == sourceWarehouseId &&
                b.MedicineId == detail.MedicineId &&
                b.BatchNumber == detail.BatchNumber, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy lô {detail.BatchNumber} tại kho nguồn để chuyển");

        var sourceBalanceBefore = sourceBatch.QuantityOnHand;
        sourceBatch.DeductStock(detail.Quantity);

        // Ghi thẻ kho xuất chuyển
        dbContext.StockLedgers.Add(new StockLedger
        {
            LedgerCode = $"TK-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
            WarehouseId = sourceWarehouseId,
            MedicineId = detail.MedicineId,
            BatchId = sourceBatch.Id,
            BatchNumber = sourceBatch.BatchNumber,
            ExpiryDate = sourceBatch.ExpiryDate,
            TransactionType = LedgerTransactionType.TransferOut,
            DocumentNumber = doc.DocumentNumber,
            Quantity = -detail.Quantity,
            BalanceBefore = sourceBalanceBefore,
            BalanceAfter = sourceBatch.QuantityOnHand,
            UnitPrice = detail.UnitPrice > 0 ? detail.UnitPrice : sourceBatch.InboundPrice,
            TransactionDate = now,
            CreatedBy = doc.ApprovedBy ?? doc.CreatedBy,
            Note = $"Xuất chuyển sang kho: {targetWarehouseId}"
        });

        // 2. Tăng kho đích
        var targetBatch = await dbContext.InventoryBatches
            .FirstOrDefaultAsync(b =>
                b.WarehouseId == targetWarehouseId &&
                b.MedicineId == detail.MedicineId &&
                b.BatchNumber == detail.BatchNumber, ct);

        decimal targetBalanceBefore = 0;
        if (targetBatch != null)
        {
            targetBalanceBefore = targetBatch.QuantityOnHand;
            targetBatch.AddStock(detail.Quantity);
        }
        else
        {
            targetBatch = new InventoryBatch
            {
                WarehouseId = targetWarehouseId,
                MedicineId = detail.MedicineId,
                BatchNumber = sourceBatch.BatchNumber,
                ExpiryDate = sourceBatch.ExpiryDate,
                ManufactureDate = sourceBatch.ManufactureDate,
                InboundPrice = sourceBatch.InboundPrice,
                SalePrice = sourceBatch.SalePrice,
                QuantityOnHand = detail.Quantity,
                QuantityReserved = 0
            };
            dbContext.InventoryBatches.Add(targetBatch);
        }

        // Ghi thẻ kho nhập chuyển
        dbContext.StockLedgers.Add(new StockLedger
        {
            LedgerCode = $"TK-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
            WarehouseId = targetWarehouseId,
            MedicineId = detail.MedicineId,
            BatchId = targetBatch.Id,
            BatchNumber = targetBatch.BatchNumber,
            ExpiryDate = targetBatch.ExpiryDate,
            TransactionType = LedgerTransactionType.TransferIn,
            DocumentNumber = doc.DocumentNumber,
            Quantity = detail.Quantity,
            BalanceBefore = targetBalanceBefore,
            BalanceAfter = targetBalanceBefore + detail.Quantity,
            UnitPrice = detail.UnitPrice > 0 ? detail.UnitPrice : sourceBatch.InboundPrice,
            TransactionDate = now,
            CreatedBy = doc.ApprovedBy ?? doc.CreatedBy,
            Note = $"Nhận chuyển từ kho: {sourceWarehouseId}"
        });
    }
}

