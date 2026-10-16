using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ExamStation : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // ST_PHYSICAL, ST_INTERNAL, ST_SURGERY, ST_EYE, ST_ENT, ST_DENTAL, ST_DERMA, ST_GYNECOL
    public string Name { get; set; } = string.Empty; // Trạm Thể lực, Trạm Nội, Trạm Ngoại...
    public string SpecialtyCode { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public Guid? DefaultDoctorId { get; set; }
    public string? DefaultDoctorName { get; set; }
    public string? FormSchemaJson { get; set; } // JSON Schema định nghĩa form động cho trạm
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
    public ICollection<StationResult> StationResults { get; set; } = new List<StationResult>();
}

