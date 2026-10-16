using HospitalMS.Pharmacy.Domain.Entities;

namespace HospitalMS.Pharmacy.Domain.Services;

public record FefoAllocationResult(
    Guid BatchId,
    string BatchNumber,
    DateTime ExpiryDate,
    decimal AllocatedQuantity,
    decimal UnitPrice
);

public static class FefoAllocationService
{
    /// <summary>
    /// Thuật toán FEFO (First Expired, First Out): Tự động phân bổ số lượng cần xuất từ các lô có hạn dùng gần nhất
    /// </summary>
    public static List<FefoAllocationResult> Allocate(
        IEnumerable<InventoryBatch> batches,
        decimal requiredQuantity,
        DateTime? currentDate = null)
    {
        if (requiredQuantity <= 0)
        {
            throw new ArgumentException("Số lượng yêu cầu phân bổ phải lớn hơn 0", nameof(requiredQuantity));
        }

        var now = currentDate ?? DateTime.UtcNow;

        // Chỉ lấy các lô còn hạn dùng và còn tồn khả dụng
        var eligibleBatches = batches
            .Where(b => b.ExpiryDate > now && b.AvailableQuantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.CreatedAt)
            .ToList();

        var totalAvailable = eligibleBatches.Sum(b => b.AvailableQuantity);
        if (totalAvailable < requiredQuantity)
        {
            throw new InvalidOperationException(
                $"Không đủ tồn kho khả dụng để xuất theo quy tắc FEFO. Tồn khả dụng: {totalAvailable:N0}, Yêu cầu xuất: {requiredQuantity:N0}");
        }

        var results = new List<FefoAllocationResult>();
        var remainingToAllocate = requiredQuantity;

        foreach (var batch in eligibleBatches)
        {
            if (remainingToAllocate <= 0) break;

            var take = Math.Min(batch.AvailableQuantity, remainingToAllocate);
            results.Add(new FefoAllocationResult(
                BatchId: batch.Id,
                BatchNumber: batch.BatchNumber,
                ExpiryDate: batch.ExpiryDate,
                AllocatedQuantity: take,
                UnitPrice: batch.SalePrice
            ));

            remainingToAllocate -= take;
        }

        return results;
    }
}

