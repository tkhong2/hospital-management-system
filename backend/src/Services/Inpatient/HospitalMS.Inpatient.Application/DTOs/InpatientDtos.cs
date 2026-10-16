using HospitalMS.Inpatient.Domain.Enums;

namespace HospitalMS.Inpatient.Application.DTOs;

public record DepartmentRoomDto(
    Guid Id,
    string DepartmentCode,
    string DepartmentName,
    string RoomNumber,
    string RoomName,
    int Floor,
    string? Notes,
    List<HospitalBedDto> Beds
);

public record HospitalBedDto(
    Guid Id,
    Guid RoomId,
    string RoomNumber,
    string RoomName,
    string DepartmentName,
    string BedCode,
    string BedName,
    BedType BedType,
    BedStatus Status,
    Guid? CurrentAdmissionId,
    string? PatientCode,
    string? PatientName,
    string? InitialDiagnosis,
    DateTime? AdmissionTime,
    decimal DailyPrice,
    decimal InsuranceCoveredPrice
);

public record BedMapSummaryDto(
    int TotalBeds,
    int EmptyBeds,
    int OccupiedBeds,
    int CleaningBeds,
    int MaintenanceBeds,
    decimal OccupancyRate,
    List<DepartmentRoomDto> Rooms
);

public record InpatientAdmissionDto(
    Guid Id,
    string AdmissionCode,
    Guid PatientId,
    string PatientCode,
    string PatientName,
    DateTime? DateOfBirth,
    string Gender,
    string? HealthInsuranceNumber,
    string? RelativeContact,
    DateTime AdmissionTime,
    string DepartmentCode,
    string DepartmentName,
    Guid? CurrentBedId,
    string? CurrentBedCode,
    string? AdmittingDoctorName,
    string InitialDiagnosis,
    string PrimaryIcd10Code,
    string PrimaryIcd10Name,
    AdmissionStatus Status,
    decimal TotalAdvancePayment,
    decimal TotalInpatientExpenses,
    DateTime? DischargeTime,
    string? DischargeDiagnosis,
    string? DischargeIcd10Code,
    DischargeOutcome? Outcome,
    string? DischargeSummaryNotes,
    string? DischargeDoctorName,
    List<BedTransferHistoryDto> TransferHistories,
    List<MedicalOrderDto> MedicalOrders,
    List<VitalSignDto> VitalSigns,
    List<CareNoteDto> CareNotes,
    List<AdvancePaymentDto> AdvancePayments
);

public record BedTransferHistoryDto(
    Guid Id,
    Guid AdmissionId,
    Guid? FromBedId,
    string? FromBedCode,
    Guid ToBedId,
    string ToBedCode,
    string? FromDepartmentName,
    string ToDepartmentName,
    DateTime TransferTime,
    string Reason,
    string? TransferredBy
);

public record MedicalOrderDto(
    Guid Id,
    Guid AdmissionId,
    DateTime OrderDate,
    string DoctorName,
    string Diagnosis,
    CareLevel CareLevel,
    string? DietPlan,
    string MedicinesContent,
    string? ServicesContent,
    string? SpecialInstructions,
    bool IsExecuted,
    string? ExecutedByNurse,
    DateTime? ExecutedTime
);

public record VitalSignDto(
    Guid Id,
    Guid AdmissionId,
    DateTime RecordTime,
    decimal Pulse,
    decimal Temperature,
    decimal BloodPressureSystolic,
    decimal BloodPressureDiastolic,
    decimal BreathingRate,
    decimal SpO2,
    decimal? Weight,
    string? Notes,
    string? RecordedByNurse
);

public record CareNoteDto(
    Guid Id,
    Guid AdmissionId,
    DateTime NoteTime,
    string ClinicalEvolution,
    string NursingActions,
    string Evaluation,
    string NurseName
);

public record AdvancePaymentDto(
    Guid Id,
    Guid AdmissionId,
    string ReceiptNumber,
    DateTime PaymentDate,
    decimal Amount,
    string PaymentMethod,
    string CashierName,
    string? Notes
);

