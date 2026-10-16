using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Domain.Enums;

namespace HospitalMS.Catalog.Domain.Entities;

public class MedicalService : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; } = ServiceType.Examination;
    public string? UnitCode { get; set; }
    public string? UnitName { get; set; }
    public decimal Price { get; set; }
    public decimal HealthInsurancePrice { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
