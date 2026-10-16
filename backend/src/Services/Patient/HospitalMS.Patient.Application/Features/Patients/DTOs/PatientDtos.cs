using HospitalMS.Patient.Domain.Enums;

namespace HospitalMS.Patient.Application.Features.Patients.DTOs;

public record PatientDto(
    Guid Id,
    string PatientCode,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber,
    string? HealthInsuranceNumber,
    string? PhoneNumber,
    string? Email,
    string? DetailAddress,
    string? WardCode,
    string? WardName,
    string? DistrictCode,
    string? DistrictName,
    string? ProvinceCode,
    string? ProvinceName,
    string FullAddress,
    string? Ethnic,
    string? Nationality,
    string? Career,
    BloodType BloodType,
    string? Allergies,
    string? Note,
    bool IsMerged,
    Guid? MergedToPatientId,
    DateTime CreatedAt
);

public record PatientListItemDto(
    Guid Id,
    string PatientCode,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber,
    string? HealthInsuranceNumber,
    string? PhoneNumber,
    string FullAddress,
    BloodType BloodType,
    bool IsMerged,
    DateTime CreatedAt
);

public record DuplicateCandidateDto(
    Guid Id,
    string PatientCode,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber,
    string? HealthInsuranceNumber,
    string? PhoneNumber,
    string Reason
);

