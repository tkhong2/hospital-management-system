using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.Common.Interfaces;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Domain.Entities;
using HospitalMS.Inpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Inpatient.Application.Features.Clinical;

public record CreateMedicalOrderCommand(
    Guid AdmissionId,
    string DoctorName,
    string Diagnosis,
    CareLevel CareLevel,
    string? DietPlan,
    string MedicinesContent,
    string? ServicesContent,
    string? SpecialInstructions
) : IRequest<Result<MedicalOrderDto>>;

public class CreateMedicalOrderHandler(IInpatientDbContext dbContext)
    : IRequestHandler<CreateMedicalOrderCommand, Result<MedicalOrderDto>>
{
    public async Task<Result<MedicalOrderDto>> Handle(CreateMedicalOrderCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<MedicalOrderDto>("Không tìm thấy hồ sơ bệnh án");

        var order = new InpatientMedicalOrder
        {
            AdmissionId = admission.Id,
            OrderDate = DateTime.UtcNow,
            DoctorName = request.DoctorName,
            Diagnosis = request.Diagnosis,
            CareLevel = request.CareLevel,
            DietPlan = request.DietPlan,
            MedicinesContent = request.MedicinesContent,
            ServicesContent = request.ServicesContent,
            SpecialInstructions = request.SpecialInstructions,
            IsExecuted = false
        };

        dbContext.MedicalOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new MedicalOrderDto(
            order.Id,
            order.AdmissionId,
            order.OrderDate,
            order.DoctorName,
            order.Diagnosis,
            order.CareLevel,
            order.DietPlan,
            order.MedicinesContent,
            order.ServicesContent,
            order.SpecialInstructions,
            order.IsExecuted,
            order.ExecutedByNurse,
            order.ExecutedTime
        );

        return Result.Success(dto);
    }
}

public record RecordVitalSignCommand(
    Guid AdmissionId,
    decimal Pulse,
    decimal Temperature,
    decimal BloodPressureSystolic,
    decimal BloodPressureDiastolic,
    decimal BreathingRate,
    decimal SpO2,
    decimal? Weight,
    string? Notes,
    string? RecordedByNurse
) : IRequest<Result<VitalSignDto>>;

public class RecordVitalSignHandler(IInpatientDbContext dbContext)
    : IRequestHandler<RecordVitalSignCommand, Result<VitalSignDto>>
{
    public async Task<Result<VitalSignDto>> Handle(RecordVitalSignCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<VitalSignDto>("Không tìm thấy hồ sơ bệnh án");

        var vital = new InpatientVitalSign
        {
            AdmissionId = admission.Id,
            RecordTime = DateTime.UtcNow,
            Pulse = request.Pulse,
            Temperature = request.Temperature,
            BloodPressureSystolic = request.BloodPressureSystolic,
            BloodPressureDiastolic = request.BloodPressureDiastolic,
            BreathingRate = request.BreathingRate,
            SpO2 = request.SpO2,
            Weight = request.Weight,
            Notes = request.Notes,
            RecordedByNurse = request.RecordedByNurse
        };

        dbContext.VitalSigns.Add(vital);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new VitalSignDto(
            vital.Id,
            vital.AdmissionId,
            vital.RecordTime,
            vital.Pulse,
            vital.Temperature,
            vital.BloodPressureSystolic,
            vital.BloodPressureDiastolic,
            vital.BreathingRate,
            vital.SpO2,
            vital.Weight,
            vital.Notes,
            vital.RecordedByNurse
        );

        return Result.Success(dto);
    }
}

public record CreateCareNoteCommand(
    Guid AdmissionId,
    string ClinicalEvolution,
    string NursingActions,
    string Evaluation,
    string NurseName
) : IRequest<Result<CareNoteDto>>;

public class CreateCareNoteHandler(IInpatientDbContext dbContext)
    : IRequestHandler<CreateCareNoteCommand, Result<CareNoteDto>>
{
    public async Task<Result<CareNoteDto>> Handle(CreateCareNoteCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<CareNoteDto>("Không tìm thấy hồ sơ bệnh án");

        var note = new InpatientCareNote
        {
            AdmissionId = admission.Id,
            NoteTime = DateTime.UtcNow,
            ClinicalEvolution = request.ClinicalEvolution,
            NursingActions = request.NursingActions,
            Evaluation = request.Evaluation,
            NurseName = request.NurseName
        };

        dbContext.CareNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CareNoteDto(
            note.Id,
            note.AdmissionId,
            note.NoteTime,
            note.ClinicalEvolution,
            note.NursingActions,
            note.Evaluation,
            note.NurseName
        );

        return Result.Success(dto);
    }
}

public record RecordAdvancePaymentCommand(
    Guid AdmissionId,
    decimal Amount,
    string PaymentMethod,
    string CashierName,
    string? Notes
) : IRequest<Result<AdvancePaymentDto>>;

public class RecordAdvancePaymentHandler(IInpatientDbContext dbContext)
    : IRequestHandler<RecordAdvancePaymentCommand, Result<AdvancePaymentDto>>
{
    public async Task<Result<AdvancePaymentDto>> Handle(RecordAdvancePaymentCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<AdvancePaymentDto>("Không tìm thấy hồ sơ bệnh án");

        var payment = new InpatientAdvancePayment
        {
            AdmissionId = admission.Id,
            ReceiptNumber = $"TU-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(100, 999)}",
            PaymentDate = DateTime.UtcNow,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            CashierName = request.CashierName,
            Notes = request.Notes
        };

        admission.TotalAdvancePayment += request.Amount;

        dbContext.AdvancePayments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new AdvancePaymentDto(
            payment.Id,
            payment.AdmissionId,
            payment.ReceiptNumber,
            payment.PaymentDate,
            payment.Amount,
            payment.PaymentMethod,
            payment.CashierName,
            payment.Notes
        );

        return Result.Success(dto);
    }
}

