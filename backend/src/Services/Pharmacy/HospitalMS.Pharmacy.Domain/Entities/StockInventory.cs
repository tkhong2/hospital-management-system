using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class StockInventory : AuditableEntity
{
    public string InventoryCode { get; set; } = string.Empty; // KK2026-0001
    public Guid WarehouseId { get; set; }
    public DateTime InventoryDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled
    public string? CouncilMembers { get; set; } // Thành viên hội đồng kiểm kê
    public string? Note { get; set; }
    public string? BalancedBy { get; set; }
    public DateTime? BalancedAt { get; set; }

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockInventoryDetail> Details { get; set; } = new List<StockInventoryDetail>();
}

public class StockInventoryDetail : BaseEntity
{
    public Guid InventoryId { get; set; }
    public Guid MedicineId { get; set; }
    public Guid? BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal SystemQuantity { get; set; } // Tồn sổ sách hệ thống
    public decimal ActualQuantity { get; set; } // Tồn kiểm đếm thực tế
    public decimal DifferenceQuantity => ActualQuantity - SystemQuantity; // Chênh lệch thừa (+) hoặc thiếu (-)
    public decimal UnitPrice { get; set; }
    public string? DifferenceReason { get; set; } // Hao hụt tự nhiên, vỡ hỏng, thừa chưa nhập...

    // Navigation
    public StockInventory Inventory { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}

