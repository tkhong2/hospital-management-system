using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class PackageService : BaseEntity
{
    public Guid PackageId { get; set; }
    public Guid MedicalServiceId { get; set; } // Reference to CatalogService MedicalService.Id
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public LabOrderType? ServiceType { get; set; }
    public Guid? ExamStationId { get; set; } // Trạm phụ trách khám/thực hiện
    public decimal Price { get; set; }
    public bool IsRequired { get; set; } = true;
    public int OrderIndex { get; set; }

    // Navigation
    public ExamPackage Package { get; set; } = null!;
    public ExamStation? ExamStation { get; set; }
}

