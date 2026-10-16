using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class LabResult : AuditableEntity
{
    public Guid LabOrderId { get; set; }
    public Guid MedicalServiceId { get; set; }
    public string TestCode { get; set; } = string.Empty; // GLU, CHOL, TRIG, AST, ALT, XQ_CHEST...
    public string TestName { get; set; } = string.Empty; // Định lượng Glucose máu, Chụp X-quang...
    public string? Value { get; set; } // "5.8", "Âm tính", "Không thấy tổn thương..."
    public decimal? NumericValue { get; set; } // 5.8
    public string? Unit { get; set; } // mmol/L, mg/dL, U/L
    public Guid? ReferenceRangeId { get; set; }
    public string? NormalRangeText { get; set; } // "3.9 - 6.4"
    public bool IsAbnormal { get; set; } // Cảnh báo bất thường
    public AbnormalFlag AbnormalFlag { get; set; } = AbnormalFlag.Normal;
    public string? ImageUrlsJson { get; set; } // JSON array các link ảnh DICOM / PACS / JPG của CĐHA
    public DateTime? PerformedAt { get; set; }
    public string? PerformedBy { get; set; } // Kỹ thuật viên / Bác sĩ CĐHA
    public string? Notes { get; set; }

    // Navigation
    public LabOrder LabOrder { get; set; } = null!;
    public ReferenceRange? ReferenceRange { get; set; }
}

