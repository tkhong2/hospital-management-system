using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class StockDocument : AuditableEntity
{
    public string DocumentNumber { get; set; } = string.Empty; // PN2026-0001, PX2026-0001, PC2026-0001
    public DocumentType DocumentType { get; set; } = DocumentType.Inbound;
    public Guid? SourceWarehouseId { get; set; } // Kho xuất (dùng cho chuyển kho / xuất hủy)
    public Guid? TargetWarehouseId { get; set; } // Kho nhập (dùng cho nhập kho / nhận chuyển kho)
    public string? SupplierName { get; set; } // Nhà cung cấp (CTY Dược TW1, Hậu Giang...)
    public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    public Warehouse? SourceWarehouse { get; set; }
    public Warehouse? TargetWarehouse { get; set; }
    public ICollection<StockDocumentDetail> Details { get; set; } = new List<StockDocumentDetail>();
}

public class StockDocumentDetail : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Guid MedicineId { get; set; }
    public Guid? SourceBatchId { get; set; } // Lô nguồn khi xuất kho / chuyển kho
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string? Note { get; set; }

    // Navigation
    public StockDocument Document { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}

