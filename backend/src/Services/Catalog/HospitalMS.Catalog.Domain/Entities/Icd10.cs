using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Catalog.Domain.Entities;

public class Icd10 : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string ChapterCode { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

