using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class StockLedger : BaseEntity
{
    public string LedgerCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid MedicineId { get; set; }
    public Guid? BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public LedgerTransactionType TransactionType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty; // Mã phiếu nhập / xuất / đơn thuốc
    public decimal Quantity { get; set; } // Dương nếu nhập/tăng kho, Âm nếu xuất/giảm kho
    public decimal BalanceBefore { get; set; } // Số dư tồn trước giao dịch
    public decimal BalanceAfter { get; set; } // Số dư tồn sau giao dịch
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount => Math.Abs(Quantity) * UnitPrice;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? Note { get; set; }

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}

