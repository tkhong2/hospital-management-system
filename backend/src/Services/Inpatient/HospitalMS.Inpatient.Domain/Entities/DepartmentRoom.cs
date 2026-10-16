using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Domain.Enums;

namespace HospitalMS.Inpatient.Domain.Entities;

public class DepartmentRoom : AuditableEntity
{
    public string DepartmentCode { get; set; } = string.Empty; // Vd: KHOA_NOI, KHOA_NGOAI
    public string DepartmentName { get; set; } = string.Empty; // Vd: Khoa Nội Tổng Hợp
    public string RoomNumber { get; set; } = string.Empty;     // Vd: P201
    public string RoomName { get; set; } = string.Empty;       // Vd: Phòng Điều Trị Tích Cực
    public int Floor { get; set; } = 2;
    public string? Notes { get; set; }

    public ICollection<HospitalBed> Beds { get; set; } = new List<HospitalBed>();
}

public class HospitalBed : AuditableEntity
{
    public Guid RoomId { get; set; }
    public DepartmentRoom Room { get; set; } = null!;

    public string BedCode { get; set; } = string.Empty; // Vd: G-201-01
    public string BedName { get; set; } = string.Empty; // Vd: Giường 01
    public BedType BedType { get; set; } = BedType.Standard;
    public BedStatus Status { get; set; } = BedStatus.Empty;
    public Guid? CurrentAdmissionId { get; set; }
    public InpatientAdmission? CurrentAdmission { get; set; }
    public decimal DailyPrice { get; set; } = 250000m;
    public decimal InsuranceCoveredPrice { get; set; } = 200000m;
    public string? Notes { get; set; }
}

