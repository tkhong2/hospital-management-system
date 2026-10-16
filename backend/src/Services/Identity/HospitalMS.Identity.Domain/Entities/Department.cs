using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Identity.Domain.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public List<User> Users { get; set; } = [];
}

