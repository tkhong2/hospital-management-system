using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamPackage : AuditableEntity
{
    public Guid? ContractId { get; set; } // Null nếu là gói chuẩn tái sử dụng
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TargetGender TargetGender { get; set; } = TargetGender.All;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PackagePrice { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ExamContract? Contract { get; set; }
    public ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
    public ICollection<ExamRegistration> Registrations { get; set; } = new List<ExamRegistration>();
}

