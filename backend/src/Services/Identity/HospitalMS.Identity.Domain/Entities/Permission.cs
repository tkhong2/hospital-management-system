using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Identity.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g. "healthexam.contract.create", "users.view"
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty; // e.g. "HEALTH_EXAM", "IDENTITY", "OUTPATIENT"
    public string? Description { get; set; }

    public List<RolePermission> RolePermissions { get; set; } = [];
}

