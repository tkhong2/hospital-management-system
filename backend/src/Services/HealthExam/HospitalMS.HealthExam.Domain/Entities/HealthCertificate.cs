using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class HealthCertificate : AuditableEntity
{
    public Guid VisitId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty; // GKSK-2026-00012
    public CertificateType CertificateType { get; set; } = CertificateType.GeneralTT14;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; } // Thường có giá trị 12 tháng kể từ ngày ký
    public string IssuedByHospitalName { get; set; } = "Bệnh viện Đa khoa Quốc tế";
    public string DirectorSignatureName { get; set; } = string.Empty; // Người đại diện cơ sở y tế ký duyệt
    public string QrVerifyCode { get; set; } = string.Empty; // Mã tra cứu / xác thực online
    public string? PdfUrl { get; set; } // Đường dẫn file PDF bản ký số
    public bool IsDelivered { get; set; } = false; // Đã trả kết quả
    public DateTime? DeliveredAt { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.AtCounter;

    // Navigation
    public ExamVisit Visit { get; set; } = null!;
}

