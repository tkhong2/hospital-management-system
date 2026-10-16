using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class InventoryBatch : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Guid MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty; // Số lô sản xuất (VD: L240801)
    public DateTime ExpiryDate { get; set; } // Hạn dùng (Quan trọng nhất cho FEFO)
    public DateTime? ManufactureDate { get; set; } // Ngày sản xuất
    public decimal InboundPrice { get; set; } // Giá nhập
    public decimal SalePrice { get; set; } // Giá bán
    public decimal QuantityOnHand { get; set; } // Tồn kho vật lý hiện tại
    public decimal QuantityReserved { get; set; } // Số lượng đang giữ chỗ đơn thuốc chờ xuất
    public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;

    /// <summary>
    /// Concurrency Token phục vụ Optimistic Concurrency Control khi nhiều quầy xuất đồng thời
    /// </summary>
    public int ConcurrencyVersion { get; set; } = 1;

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;

    public void AddStock(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0", nameof(quantity));
        QuantityOnHand += quantity;
        ConcurrencyVersion++;
    }

    public void ReserveStock(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Số lượng giữ chỗ phải lớn hơn 0", nameof(quantity));
        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException($"Lô {BatchNumber} không đủ số lượng khả dụng. Còn lại: {AvailableQuantity}, Yêu cầu: {quantity}");
        }
        QuantityReserved += quantity;
        ConcurrencyVersion++;
    }

    public void ReleaseReservation(decimal quantity)
    {
        if (quantity <= 0) return;
        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
        ConcurrencyVersion++;
    }

    public void DeductStock(decimal quantity, bool wasReserved = false)
    {
        if (quantity <= 0) throw new ArgumentException("Số lượng xuất phải lớn hơn 0", nameof(quantity));
        if (QuantityOnHand < quantity)
        {
            throw new InvalidOperationException($"Lô {BatchNumber} không đủ tồn kho thực tế để xuất. Tồn: {QuantityOnHand}, Xuất: {quantity}");
        }

        QuantityOnHand -= quantity;
        if (wasReserved)
        {
            QuantityReserved = Math.Max(0, QuantityReserved - quantity);
        }
        ConcurrencyVersion++;
    }
}

