using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamVisit : AuditableEntity
{
    public Guid RegistrationId { get; set; }
    public string VisitCode { get; set; } = string.Empty; // LK2026-00001
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public int QueueNumber { get; set; } // Số thứ tự khám trong ngày
    public Guid? CurrentStationId { get; set; } // Trạm khám tiếp theo đang chờ
    public VisitStatus Status { get; set; } = VisitStatus.CheckedIn;

    // Physical Vital Signs (Thể lực)
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? Bmi { get; set; }
    public int? PulseRate { get; set; } // Mạch (lần/phút)
    public int? BloodPressureSystolic { get; set; } // Huyết áp tâm thu (mmHg)
    public int? BloodPressureDiastolic { get; set; } // Huyết áp tâm trương (mmHg)

    // Navigation
    public ExamRegistration Registration { get; set; } = null!;
    public ExamStation? CurrentStation { get; set; }
    public ICollection<StationResult> StationResults { get; set; } = new List<StationResult>();
    public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
    public ExamConclusion? Conclusion { get; set; }
    public HealthCertificate? Certificate { get; set; }
}

