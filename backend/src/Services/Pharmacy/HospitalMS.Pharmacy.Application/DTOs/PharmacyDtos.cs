using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Application.DTOs;

public record MedicineDto(
    Guid Id,
    string Code,
    string Name,
    string? RegistrationNumber,
    string ActiveIngredient,
    string Concentration,
    string UnitName,
    string? PackageSpecification,
    string? RouteOfAdministration,
    string? MedicineGroup,
    MedicineType MedicineType,
    SpecialControlType SpecialControl,
    decimal InboundPrice,
    decimal SalePrice,
    decimal? InsurancePrice,
    decimal MinStockLevel,
    decimal MaxStockLevel,
    bool IsActive
);

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    WarehouseType WarehouseType,
    string WarehouseTypeName,
    Guid? DepartmentId,
    string? DepartmentName,
    string? Location,
    string? ManagerName,
    bool IsActive
);

public record InventoryBatchDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string UnitName,
    string BatchNumber,
    DateTime ExpiryDate,
    DateTime? ManufactureDate,
    decimal InboundPrice,
    decimal SalePrice,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal AvailableQuantity,
    int DaysUntilExpiry,
    bool IsNearExpiry,
    bool IsExpired
);

public record RealtimeStockDto(
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string ActiveIngredient,
    string Concentration,
    string UnitName,
    SpecialControlType SpecialControl,
    decimal TotalOnHand,
    decimal TotalReserved,
    decimal TotalAvailable,
    decimal MinStockLevel,
    bool IsUnderMinStock,
    List<InventoryBatchDto> Batches
);

public record StockAlertDto(
    string AlertType, // "Outdated", "NearExpiry", "UnderMinStock"
    string Severity, // "Critical", "Warning"
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    Guid WarehouseId,
    string WarehouseName,
    string? BatchNumber,
    DateTime? ExpiryDate,
    decimal CurrentQuantity,
    decimal? ThresholdQuantity,
    string Message
);

public record StockDocumentItemDto(
    Guid MedicineId,
    string? MedicineCode,
    string? MedicineName,
    Guid? SourceBatchId,
    string BatchNumber,
    DateTime ExpiryDate,
    DateTime? ManufactureDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Note
);

public record StockDocumentDto(
    Guid Id,
    string DocumentNumber,
    DocumentType DocumentType,
    string DocumentTypeName,
    Guid? SourceWarehouseId,
    string? SourceWarehouseName,
    Guid? TargetWarehouseId,
    string? TargetWarehouseName,
    string? SupplierName,
    DateTime DocumentDate,
    DocumentStatus Status,
    string StatusName,
    decimal TotalAmount,
    string? Note,
    string? CreatedBy,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    List<StockDocumentItemDto> Details
);

public record CreateStockDocumentDto(
    DocumentType DocumentType,
    Guid? SourceWarehouseId,
    Guid? TargetWarehouseId,
    string? SupplierName,
    string? Note,
    List<StockDocumentItemDto> Details
);

public record PrescriptionOrderDetailDto(
    Guid Id,
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string UnitName,
    decimal QuantityRequested,
    decimal QuantityDispensed,
    decimal UnitPrice,
    decimal TotalPrice,
    string? UsageInstructions,
    Guid? AllocatedBatchId,
    string? AllocatedBatchNumber,
    DateTime? AllocatedExpiryDate
);

public record PrescriptionOrderDto(
    Guid Id,
    string PrescriptionCode,
    PrescriptionSourceType SourceType,
    string SourceTypeName,
    Guid? SourceReferenceId,
    Guid? PatientId,
    string PatientCode,
    string PatientName,
    string? DoctorName,
    string? Diagnosis,
    DateTime PrescribedAt,
    Guid WarehouseId,
    string WarehouseName,
    PrescriptionStatus Status,
    string StatusName,
    decimal TotalAmount,
    string? DispensedBy,
    DateTime? DispensedAt,
    string? Note,
    List<PrescriptionOrderDetailDto> Details
);

public record StockLedgerDto(
    Guid Id,
    string LedgerCode,
    Guid WarehouseId,
    string WarehouseName,
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string BatchNumber,
    DateTime? ExpiryDate,
    LedgerTransactionType TransactionType,
    string TransactionTypeName,
    string DocumentNumber,
    decimal Quantity,
    decimal BalanceBefore,
    decimal BalanceAfter,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTime TransactionDate,
    string? CreatedBy,
    string? Note
);

public record StockReportItemDto(
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string UnitName,
    decimal OpeningBalance,    // Tồn đầu kỳ
    decimal InboundQuantity,   // Nhập trong kỳ
    decimal OutboundQuantity,  // Xuất trong kỳ
    decimal ClosingBalance,    // Tồn cuối kỳ
    decimal UnitPrice,
    decimal ClosingAmount      // Giá trị tồn cuối kỳ
);

