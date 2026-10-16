using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.HealthExam.Domain.Entities;

public class Company : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ExamContract> Contracts { get; set; } = new List<ExamContract>();
}

