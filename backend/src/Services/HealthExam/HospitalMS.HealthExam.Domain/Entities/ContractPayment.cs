using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ContractPayment : AuditableEntity
{
    public Guid ContractId { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public string? TransactionReference { get; set; }
    public string? Note { get; set; }

    // Navigation
    public ExamContract Contract { get; set; } = null!;
}

