using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Catalog.Domain.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Clinical"; // Clinical (Lâm sàng), Paraclinical (Cận lâm sàng), Administrative (Hành chính)
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Specialty : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Unit : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdministrativeDivision : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = "Province"; // Province, District, Ward
    public string? ParentCode { get; set; }
}

