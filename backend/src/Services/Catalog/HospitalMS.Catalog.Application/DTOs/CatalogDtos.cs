using HospitalMS.Catalog.Domain.Enums;

namespace HospitalMS.Catalog.Application.DTOs;

public record Icd10Dto(
    Guid Id,
    string Code,
    string Name,
    string? NameEn,
    string ChapterCode,
    string ChapterName,
    bool IsActive
);

public record MedicalServiceDto(
    Guid Id,
    string Code,
    string Name,
    ServiceType ServiceType,
    string? UnitCode,
    string? UnitName,
    decimal Price,
    decimal HealthInsurancePrice,
    Guid? DepartmentId,
    string? DepartmentName,
    string? Description,
    bool IsActive
);

public record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string? PhoneNumber,
    string? Location,
    bool IsActive
);

public record SpecialtyDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record UnitDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record AdministrativeDivisionDto(
    Guid Id,
    string Code,
    string Name,
    string Level,
    string? ParentCode
);

