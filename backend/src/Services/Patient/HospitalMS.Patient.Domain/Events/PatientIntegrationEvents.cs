using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Domain.Enums;

namespace HospitalMS.Patient.Domain.Events;

public record PatientCreatedIntegrationEvent(
    Guid PatientId,
    string PatientCode,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber,
    string? HealthInsuranceNumber,
    string? PhoneNumber,
    string? FullAddress
) : IntegrationEvent;

public record PatientUpdatedIntegrationEvent(
    Guid PatientId,
    string PatientCode,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber,
    string? HealthInsuranceNumber,
    string? PhoneNumber,
    string? FullAddress
) : IntegrationEvent;

public record PatientMergedIntegrationEvent(
    Guid SourcePatientId,
    string SourcePatientCode,
    Guid TargetPatientId,
    string TargetPatientCode,
    string Reason
) : IntegrationEvent;

