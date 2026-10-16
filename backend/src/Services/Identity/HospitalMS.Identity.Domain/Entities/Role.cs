using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Identity.Domain.Entities;

public class Role : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // e.g. "Admin", "Doctor", "Nurse", "Pharmacist"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    public List<RolePermission> RolePermissions { get; set; } = [];
    public List<RoleApplication> RoleApplications { get; set; } = [];
    public List<UserRole> UserRoles { get; set; } = [];
}
