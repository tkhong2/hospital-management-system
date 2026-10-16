using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Domain.Enums;

namespace HospitalMS.Inpatient.Domain.Entities;

public class InpatientMedicalOrder : AuditableEntity
{
    public Guid AdmissionId { get; set; }
    public InpatientAdmission Admission { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public CareLevel CareLevel { get; set; } = CareLevel.Level2;
    public string? DietPlan { get; set; } // Cơm bệnh lý, cháo lỏng, hạn chế muối...
    
    // Nội dung y lệnh điều trị
    public string MedicinesContent { get; set; } = string.Empty; // Danh sách thuốc & liều dùng
    public string? ServicesContent { get; set; }                 // Xét nghiệm, chẩn đoán hình ảnh chỉ định
    public string? SpecialInstructions { get; set; }             // Theo dõi sinh hiệu mỗi 2h, dẫn lưu...

    public bool IsExecuted { get; set; }
    public string? ExecutedByNurse { get; set; }
    public DateTime? ExecutedTime { get; set; }
}

public class InpatientVitalSign : AuditableEntity
{
    public Guid AdmissionId { get; set; }
    public InpatientAdmission Admission { get; set; } = null!;

    public DateTime RecordTime { get; set; } = DateTime.UtcNow;
    public decimal Pulse { get; set; }                  // Mạch (lần/phút)
    public decimal Temperature { get; set; }            // Nhiệt độ (°C)
    public decimal BloodPressureSystolic { get; set; }  // HA tối đa (mmHg)
    public decimal BloodPressureDiastolic { get; set; } // HA tối thiểu (mmHg)
    public decimal BreathingRate { get; set; }          // Nhịp thở (lần/phút)
    public decimal SpO2 { get; set; }                   // SpO2 (%)
    public decimal? Weight { get; set; }                // Cân nặng (kg)
    public string? Notes { get; set; }
    public string? RecordedByNurse { get; set; }
}

public class InpatientCareNote : AuditableEntity
{
    public Guid AdmissionId { get; set; }
    public InpatientAdmission Admission { get; set; } = null!;

    public DateTime NoteTime { get; set; } = DateTime.UtcNow;
    public string ClinicalEvolution { get; set; } = string.Empty; // Diễn biến bệnh nhân
    public string NursingActions { get; set; } = string.Empty;    // Thực hiện y lệnh & can thiệp chăm sóc
    public string Evaluation { get; set; } = string.Empty;        // Đánh giá kết quả
    public string NurseName { get; set; } = string.Empty;
}

public class InpatientAdvancePayment : AuditableEntity
{
    public Guid AdmissionId { get; set; }
    public InpatientAdmission Admission { get; set; } = null!;

    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "CASH"; // CASH, TRANSFER, CARD
    public string CashierName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

