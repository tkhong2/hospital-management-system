using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class LabOrder : AuditableEntity
{
    public Guid VisitId { get; set; }
    public string OrderCode { get; set; } = string.Empty; // CLS2026-0001
    public LabOrderType OrderType { get; set; } = LabOrderType.Laboratory;
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public string? OrderedByDoctorName { get; set; }
    public LabOrderStatus Status { get; set; } = LabOrderStatus.Pending;
    public string? SampleCode { get; set; } // Mã vạch barcode mẫu nghiệm (ống máu, que test...)

    // Navigation
    public ExamVisit Visit { get; set; } = null!;
    public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
}

