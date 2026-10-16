using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class Warehouse : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // KC01, KL_NGOAI, TT_CAP_CUU
    public string Name { get; set; } = string.Empty;
    public WarehouseType WarehouseType { get; set; } = WarehouseType.SubWarehouse;
    public Guid? DepartmentId { get; set; } // Liên kết khoa lâm sàng (đối với tủ trực)
    public string? DepartmentName { get; set; }
    public string? Location { get; set; } // Vị trí kho (Nhà B - Tầng 1)
    public string? ManagerName { get; set; } // Thủ kho / Dược sĩ phụ trách
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();
    public ICollection<StockLedger> Ledgers { get; set; } = new List<StockLedger>();
}

