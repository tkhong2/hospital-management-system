using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamConclusion : AuditableEntity
{
    public Guid VisitId { get; set; }
    public Guid ConcludingDoctorId { get; set; }
    public string ConcludingDoctorName { get; set; } = string.Empty;
    public DateTime ConcludedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Phân loại sức khỏe chung theo Thông tư 14 (Loại I - V) hoặc Thông tư 24 (Đạt / Không đạt)
    /// </summary>
    public HealthClassification FinalClassification { get; set; } = HealthClassification.Unclassified;

    public string? MainDiseaseIcd10Code { get; set; } // Mã ICD-10 bệnh chính nếu có
    public string? MainDiseaseName { get; set; }
    public string? SecondaryDiseasesJson { get; set; } // JSON array các bệnh kèm theo
    public string DoctorConclusionText { get; set; } = string.Empty; // Kết luận tổng quan của bác sĩ
    public string? HealthAdvice { get; set; } // Lời khuyên, chế độ dinh dưỡng, rèn luyện thể lực
    public DateTime? ReExaminationDate { get; set; } // Ngày hẹn tái khám
    public bool ConsultationRequired { get; set; } // Có yêu cầu hội chẩn chuyên sâu không
    public string? ConsultationNotes { get; set; } // Biên bản ý kiến hội chẩn
    public bool IsApproved { get; set; } = false; // Đã ký duyệt kết luận
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    public ExamVisit Visit { get; set; } = null!;
}

