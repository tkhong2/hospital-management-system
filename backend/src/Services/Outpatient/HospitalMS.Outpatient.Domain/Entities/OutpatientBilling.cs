using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Domain.Entities;

public class OutpatientBilling : AuditableEntity
{
    public Guid EncounterId { get; set; }
    public OutpatientEncounter Encounter { get; set; } = null!;

    public Guid PatientId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = "CASH"; // CASH, TRANSFER, CARD, EWALLET
    public string? CashierName { get; set; }
    public BillingStatus Status { get; set; } = BillingStatus.Paid;

    public ICollection<OutpatientBillingItem> Items { get; set; } = new List<OutpatientBillingItem>();
}

public class OutpatientBillingItem : AuditableEntity
{
    public Guid BillingId { get; set; }
    public OutpatientBilling Billing { get; set; } = null!;

    public string ItemType { get; set; } = "SERVICE"; // SERVICE, MEDICINE, BED, EXAM_FEE
    public Guid ItemReferenceId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
}

