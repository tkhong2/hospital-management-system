using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Domain.Entities;

public class OutpatientReception : AuditableEntity
{
    public int QueueNumber { get; set; }
    public Guid PatientId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = "Other";
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? HealthInsuranceNumber { get; set; }
    
    public Guid ClinicRoomId { get; set; }
    public string ClinicRoomName { get; set; } = string.Empty;
    public PatientType PatientType { get; set; } = PatientType.OutpatientInsurance;
    public ReceptionStatus Status { get; set; } = ReceptionStatus.Waiting;
    public DateTime ReceptionTime { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public int Priority { get; set; } = 0; // 0: Bình thường, 1: Ưu tiên (trẻ em < 6 tuổi, người già >= 75, cấp cứu nhẹ)

    public OutpatientEncounter? Encounter { get; set; }
}

