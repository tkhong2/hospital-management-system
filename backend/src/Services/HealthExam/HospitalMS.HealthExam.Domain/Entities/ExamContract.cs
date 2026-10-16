using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamContract : AuditableEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ExamLocationType ExamLocation { get; set; } = ExamLocationType.AtHospital;
    public string? LocationAddress { get; set; }
    public int EstimatedPatientCount { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public decimal TotalContractAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingDebt => FinalAmount - TotalPaidAmount;
    public string? Note { get; set; }

    // Navigation
    public Company Company { get; set; } = null!;
    public ICollection<ExamPackage> Packages { get; set; } = new List<ExamPackage>();
    public ICollection<ExamRegistration> Registrations { get; set; } = new List<ExamRegistration>();
    public ICollection<ContractPayment> Payments { get; set; } = new List<ContractPayment>();
}

