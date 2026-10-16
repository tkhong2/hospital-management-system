using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Domain.Enums;

namespace HospitalMS.Inpatient.Domain.Entities;

public class InpatientAdmission : AuditableEntity
{
    public string AdmissionCode { get; set; } = string.Empty; // Vd: BA-2026-0001
    public Guid PatientId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = "Other";
    public string? HealthInsuranceNumber { get; set; }
    public string? RelativeContact { get; set; } // Người nhà liên hệ & SĐT

    public DateTime AdmissionTime { get; set; } = DateTime.UtcNow;
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? CurrentBedId { get; set; }
    public string? CurrentBedCode { get; set; }
    public string? AdmittingDoctorName { get; set; }
    public string InitialDiagnosis { get; set; } = string.Empty;
    public string PrimaryIcd10Code { get; set; } = string.Empty;
    public string PrimaryIcd10Name { get; set; } = string.Empty;

    public AdmissionStatus Status { get; set; } = AdmissionStatus.Admitted;
    public decimal TotalAdvancePayment { get; set; }
    public decimal TotalInpatientExpenses { get; set; }

    // Xuất viện & Tổng kết bệnh án
    public DateTime? DischargeTime { get; set; }
    public string? DischargeDiagnosis { get; set; }
    public string? DischargeIcd10Code { get; set; }
    public DischargeOutcome? Outcome { get; set; }
    public string? DischargeSummaryNotes { get; set; }
    public string? DischargeDoctorName { get; set; }

    public ICollection<BedTransferHistory> TransferHistories { get; set; } = new List<BedTransferHistory>();
    public ICollection<InpatientMedicalOrder> MedicalOrders { get; set; } = new List<InpatientMedicalOrder>();
    public ICollection<InpatientVitalSign> VitalSigns { get; set; } = new List<InpatientVitalSign>();
    public ICollection<InpatientCareNote> CareNotes { get; set; } = new List<InpatientCareNote>();
    public ICollection<InpatientAdvancePayment> AdvancePayments { get; set; } = new List<InpatientAdvancePayment>();
}

public class BedTransferHistory : AuditableEntity
{
    public Guid AdmissionId { get; set; }
    public InpatientAdmission Admission { get; set; } = null!;

    public Guid? FromBedId { get; set; }
    public string? FromBedCode { get; set; }
    public Guid ToBedId { get; set; }
    public string ToBedCode { get; set; } = string.Empty;
    public string? FromDepartmentName { get; set; }
    public string ToDepartmentName { get; set; } = string.Empty;
    public DateTime TransferTime { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public string? TransferredBy { get; set; }
}

