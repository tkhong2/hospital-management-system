using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Domain.Entities;

public class OutpatientEncounter : AuditableEntity
{
    public Guid ReceptionId { get; set; }
    public OutpatientReception Reception { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public string? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string ClinicRoomName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }

    // Quá trình bệnh lý & Tiền sử
    public string ChiefComplaint { get; set; } = string.Empty; // Lý do khám
    public string? HistoryOfPresentIllness { get; set; }        // Quá trình bệnh lý
    public string? MedicalHistory { get; set; }                // Tiền sử bản thân, dị ứng, gia đình

    // Khám lâm sàng & Sinh hiệu
    public string? PhysicalExam { get; set; }                  // Khám các cơ quan
    public string? VitalSignsSummary { get; set; }             // Tóm tắt Mạch, HA, Thân nhiệt, SpO2
    public decimal? Pulse { get; set; }                        // Mạch (lần/phút)
    public decimal? BloodPressureSystolic { get; set; }        // Huyết áp tâm thu (mmHg)
    public decimal? BloodPressureDiastolic { get; set; }       // Huyết áp tâm trương (mmHg)
    public decimal? Temperature { get; set; }                  // Thân nhiệt (độ C)
    public decimal? SpO2 { get; set; }                         // SpO2 (%)
    public decimal? Weight { get; set; }                       // Cân nặng (kg)
    public decimal? Height { get; set; }                       // Chiều cao (cm)

    // Chẩn đoán theo ICD-10
    public string PrimaryIcd10Code { get; set; } = string.Empty;
    public string PrimaryIcd10Name { get; set; } = string.Empty;
    public string? SecondaryIcd10Code { get; set; }
    public string? SecondaryIcd10Name { get; set; }
    public string? ClinicalDiagnosis { get; set; }

    // Xử trí & Lời dặn
    public TreatmentDisposition Disposition { get; set; } = TreatmentDisposition.DischargedWithPrescription;
    public string? DoctorNotes { get; set; }
    public DateTime? FollowUpDate { get; set; }

    public ICollection<OutpatientServiceOrder> ServiceOrders { get; set; } = new List<OutpatientServiceOrder>();
    public ICollection<OutpatientPrescription> Prescriptions { get; set; } = new List<OutpatientPrescription>();
    public ICollection<OutpatientBilling> Billings { get; set; } = new List<OutpatientBilling>();
}

