using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class HealthExamContract : AggregateRoot
{
    public string ContractNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsMobileExam { get; set; } // Khám lưu động tại công ty hay tại viện
    public int EstimatedPatientCount { get; set; }
    public decimal TotalContractAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingDebt => TotalContractAmount - TotalPaidAmount;
    public List<HealthExamPackage> Packages { get; set; } = [];
}

public class HealthExamPackage : AuditableEntity
{
    public Guid ContractId { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? TargetGender { get; set; } // "All", "Male", "Female", "MarriedFemale"
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public List<PackageServiceItem> ServiceItems { get; set; } = [];
}

public class PackageServiceItem : BaseEntity
{
    public Guid PackageId { get; set; }
    public Guid MedicalServiceId { get; set; } // ID từ CatalogService
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string SpecialtyCode { get; set; } = string.Empty; // Mã trạm khám (Nội, Ngoại, Mắt, TMH, RHM, Lab, CĐHA...)
    public decimal Price { get; set; }
    public bool IsRequired { get; set; } = true;
}

public class HealthExamRecord : AggregateRoot
{
    public Guid ContractId { get; set; }
    public Guid PackageId { get; set; }
    public Guid PatientId { get; set; } // MPI từ PatientService
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CitizenId { get; set; } = string.Empty; // CCCD
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public ExamType ExamType { get; set; } = ExamType.Corporate;
    public RecordStatus Status { get; set; } = RecordStatus.Pending;
    public HealthClassification Classification { get; set; } = HealthClassification.Unclassified;
    public string? DoctorConclusion { get; set; }
    public string? DoctorConsultation { get; set; }
    public string? ConcludingDoctorName { get; set; }
    public DateTime? ConcludedAt { get; set; }
}
