using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class PrescriptionOrder : AuditableEntity
{
    public string PrescriptionCode { get; set; } = string.Empty; // DT2026-0001
    public PrescriptionSourceType SourceType { get; set; } = PrescriptionSourceType.Outpatient;
    public Guid? SourceReferenceId { get; set; } // Mã khám bệnh ngoại trú / bệnh án nội trú / hồ sơ KSK
    public Guid? PatientId { get; set; } // Liên kết MPI PatientService
    public string PatientCode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? Diagnosis { get; set; } // Chẩn đoán bệnh
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;
    public Guid WarehouseId { get; set; } // Kho xuất cấp phát
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? DispensedBy { get; set; } // Dược sĩ cấp phát
    public DateTime? DispensedAt { get; set; }
    public string? Note { get; set; }

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<PrescriptionOrderDetail> Details { get; set; } = new List<PrescriptionOrderDetail>();
}

public class PrescriptionOrderDetail : BaseEntity
{
    public Guid PrescriptionOrderId { get; set; }
    public Guid MedicineId { get; set; }
    public string MedicineCode { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string UnitName { get; set; } = "Viên";
    public decimal QuantityRequested { get; set; }
    public decimal QuantityDispensed { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => QuantityRequested * UnitPrice;
    public string? UsageInstructions { get; set; } // Sáng 1v, Chiều 1v sau ăn...
    public Guid? AllocatedBatchId { get; set; } // Lô được tự động gán theo FEFO
    public string? AllocatedBatchNumber { get; set; }
    public DateTime? AllocatedExpiryDate { get; set; }

    // Navigation
    public PrescriptionOrder PrescriptionOrder { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}

