using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class StationResult : AuditableEntity
{
    public Guid VisitId { get; set; }
    public Guid StationId { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? ExaminedAt { get; set; }
    public StationStatus Status { get; set; } = StationStatus.Waiting;

    /// <summary>
    /// Dữ liệu kết quả khám động định dạng JSON theo schema của trạm
    /// (VD trạm Mắt: { "visionLeft": "10/10", "visionRight": "9/10", "colorBlindness": false })
    /// (VD trạm TMH: { "ear": "Bình thường", "nose": "Viêm mũi dị ứng", "throat": "Sạch" })
    /// </summary>
    public string? ResultDataJson { get; set; }

    public string? ClinicalConclusion { get; set; } // Kết luận sơ bộ của bác sĩ chuyên khoa
    public HealthClassification SpecialtyClassification { get; set; } = HealthClassification.Unclassified; // Phân loại I-V chuyên khoa
    public string? Notes { get; set; }

    // Navigation
    public ExamVisit Visit { get; set; } = null!;
    public ExamStation Station { get; set; } = null!;
}

