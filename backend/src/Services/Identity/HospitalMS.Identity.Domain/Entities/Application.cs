using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Identity.Domain.Entities;

public class Application : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // e.g. HEALTH_EXAM, OUTPATIENT, INPATIENT, PHARMACY, PATIENT, CATALOG, ADMIN
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<RoleApplication> RoleApplications { get; set; } = [];
}

