using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Application.DTOs;

public record ReceptionDto(
    Guid Id,
    int QueueNumber,
    Guid PatientId,
    string PatientCode,
    string PatientName,
    DateTime? DateOfBirth,
    string Gender,
    string? PhoneNumber,
    string? HealthInsuranceNumber,
    Guid ClinicRoomId,
    string ClinicRoomName,
    PatientType PatientType,
    ReceptionStatus Status,
    DateTime ReceptionTime,
    string? Reason,
    int Priority,
    Guid? EncounterId
);

public record EncounterDto(
    Guid Id,
    Guid ReceptionId,
    Guid PatientId,
    string PatientCode,
    string PatientName,
    string? DoctorId,
    string? DoctorName,
    string ClinicRoomName,
    DateTime StartTime,
    DateTime? EndTime,
    string ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? MedicalHistory,
    string? PhysicalExam,
    decimal? Pulse,
    decimal? BloodPressureSystolic,
    decimal? BloodPressureDiastolic,
    decimal? Temperature,
    decimal? SpO2,
    decimal? Weight,
    decimal? Height,
    string PrimaryIcd10Code,
    string PrimaryIcd10Name,
    string? SecondaryIcd10Code,
    string? SecondaryIcd10Name,
    string? ClinicalDiagnosis,
    TreatmentDisposition Disposition,
    string? DoctorNotes,
    DateTime? FollowUpDate,
    List<ServiceOrderDto> ServiceOrders,
    List<PrescriptionDto> Prescriptions
);

public record ServiceOrderDto(
    Guid Id,
    Guid EncounterId,
    string OrderCode,
    DateTime OrderDate,
    string? DoctorName,
    ServiceOrderStatus Status,
    decimal TotalAmount,
    decimal TotalInsuranceAmount,
    decimal TotalPatientAmount,
    List<ServiceOrderDetailDto> Details
);

public record ServiceOrderDetailDto(
    Guid Id,
    Guid ServiceId,
    string ServiceCode,
    string ServiceName,
    string ServiceGroup,
    int Quantity,
    decimal UnitPrice,
    decimal InsuranceCoverageRate,
    decimal InsuranceCoveredAmount,
    decimal PatientPayAmount,
    string? RoomToExecute,
    string? ResultSummary,
    DateTime? ResultTime,
    bool IsAbnormal
);

public record PrescriptionDto(
    Guid Id,
    Guid EncounterId,
    string PrescriptionCode,
    DateTime PrescriptionDate,
    string? DoctorName,
    string Diagnosis,
    int DaysCount,
    string? Advice,
    PrescriptionStatus Status,
    decimal TotalAmount,
    decimal TotalInsuranceAmount,
    decimal TotalPatientAmount,
    List<PrescriptionItemDto> Items
);

public record PrescriptionItemDto(
    Guid Id,
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string? ActiveIngredient,
    string? DosageForm,
    int Quantity,
    string? MorningDose,
    string? NoonDose,
    string? AfternoonDose,
    string? NightDose,
    string? UsageInstructions,
    decimal UnitPrice,
    bool IsInsuranceCovered,
    decimal InsuranceCoveredAmount,
    decimal PatientPayAmount
);

public record BillingDto(
    Guid Id,
    Guid EncounterId,
    Guid PatientId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal InsuranceCoveredAmount,
    decimal PatientPayAmount,
    decimal DiscountAmount,
    decimal AmountPaid,
    string PaymentMethod,
    string? CashierName,
    BillingStatus Status,
    List<BillingItemDto> Items
);

public record BillingItemDto(
    Guid Id,
    string ItemType,
    Guid ItemReferenceId,
    string ItemCode,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal InsuranceCoveredAmount,
    decimal PatientPayAmount
);

