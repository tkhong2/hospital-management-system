using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamRegistration : AuditableEntity
{
    public string RegistrationCode { get; set; } = string.Empty; // Mã vạch / QR (DK2026-00001)
    public Guid? ContractId { get; set; } // Null nếu là đăng ký lẻ ngoài đoàn
    public Guid PackageId { get; set; }
    public Guid? PatientId { get; set; } // Liên kết MPI PatientService
    public string? PatientCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public TargetGender Gender { get; set; } = TargetGender.Male;
    public string? IdentityCardNumber { get; set; } // Số CCCD
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? EmployeeCode { get; set; } // Mã nhân viên công ty
    public string? DepartmentName { get; set; } // Phòng ban công ty
    public string? Position { get; set; } // Chức vụ
    public ExamType ExamType { get; set; } = ExamType.Corporate;
    public string? DriverLicenseClass { get; set; } // Hạng bằng lái xe (B1, B2, C, D, E...) nếu là ExamType.Driver
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public string? PhotoUrl { get; set; } // Ảnh chân dung chụp tại tiếp đón
    public decimal ExtraServicesAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    // Navigation
    public ExamContract? Contract { get; set; }
    public ExamPackage Package { get; set; } = null!;
    public ICollection<ExamVisit> Visits { get; set; } = new List<ExamVisit>();
}

