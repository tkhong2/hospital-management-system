using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.Common.Interfaces;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Domain.Entities;
using HospitalMS.Inpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Inpatient.Application.Features.Admissions;

public record CreateAdmissionCommand(
    Guid PatientId,
    string PatientCode,
    string PatientName,
    DateTime? DateOfBirth,
    string Gender,
    string? HealthInsuranceNumber,
    string? RelativeContact,
    string DepartmentCode,
    string DepartmentName,
    Guid TargetBedId,
    string? AdmittingDoctorName,
    string InitialDiagnosis,
    string PrimaryIcd10Code,
    string PrimaryIcd10Name,
    decimal InitialAdvancePayment = 0m
) : IRequest<Result<InpatientAdmissionDto>>;

public class CreateAdmissionHandler(IInpatientDbContext dbContext)
    : IRequestHandler<CreateAdmissionCommand, Result<InpatientAdmissionDto>>
{
    public async Task<Result<InpatientAdmissionDto>> Handle(CreateAdmissionCommand request, CancellationToken cancellationToken)
    {
        var targetBed = await dbContext.Beds
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == request.TargetBedId, cancellationToken);

        if (targetBed == null)
            return Result.Failure<InpatientAdmissionDto>("Không tìm thấy giường tiếp nhận");

        if (targetBed.Status != BedStatus.Empty)
            return Result.Failure<InpatientAdmissionDto>($"Giường {targetBed.BedCode} không còn trống");

        var admissionCode = $"BA-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var admission = new InpatientAdmission
        {
            Id = Guid.NewGuid(),
            AdmissionCode = admissionCode,
            PatientId = request.PatientId,
            PatientCode = request.PatientCode,
            PatientName = request.PatientName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            HealthInsuranceNumber = request.HealthInsuranceNumber,
            RelativeContact = request.RelativeContact,
            AdmissionTime = DateTime.UtcNow,
            DepartmentCode = request.DepartmentCode,
            DepartmentName = request.DepartmentName,
            CurrentBedId = targetBed.Id,
            CurrentBedCode = targetBed.BedCode,
            AdmittingDoctorName = request.AdmittingDoctorName,
            InitialDiagnosis = request.InitialDiagnosis,
            PrimaryIcd10Code = request.PrimaryIcd10Code,
            PrimaryIcd10Name = request.PrimaryIcd10Name,
            Status = AdmissionStatus.Admitted,
            TotalAdvancePayment = request.InitialAdvancePayment,
            TotalInpatientExpenses = targetBed.DailyPrice
        };

        // Gán giường
        targetBed.Status = BedStatus.Occupied;
        targetBed.CurrentAdmissionId = admission.Id;

        // Nếu có tạm ứng ban đầu
        if (request.InitialAdvancePayment > 0)
        {
            admission.AdvancePayments.Add(new InpatientAdvancePayment
            {
                ReceiptNumber = $"TU-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(100, 999)}",
                PaymentDate = DateTime.UtcNow,
                Amount = request.InitialAdvancePayment,
                PaymentMethod = "CASH",
                CashierName = "Thu ngân nhập viện",
                Notes = "Tạm ứng viện phí khi nhập viện"
            });
        }

        dbContext.Admissions.Add(admission);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetAdmissionDetail(admission.Id, dbContext, cancellationToken);
    }

    internal static async Task<Result<InpatientAdmissionDto>> GetAdmissionDetail(Guid id, IInpatientDbContext db, CancellationToken ct)
    {
        var a = await db.Admissions
            .Include(x => x.TransferHistories)
            .Include(x => x.MedicalOrders)
            .Include(x => x.VitalSigns)
            .Include(x => x.CareNotes)
            .Include(x => x.AdvancePayments)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (a == null) return Result.Failure<InpatientAdmissionDto>("Không tìm thấy hồ sơ bệnh án");

        var dto = new InpatientAdmissionDto(
            a.Id,
            a.AdmissionCode,
            a.PatientId,
            a.PatientCode,
            a.PatientName,
            a.DateOfBirth,
            a.Gender,
            a.HealthInsuranceNumber,
            a.RelativeContact,
            a.AdmissionTime,
            a.DepartmentCode,
            a.DepartmentName,
            a.CurrentBedId,
            a.CurrentBedCode,
            a.AdmittingDoctorName,
            a.InitialDiagnosis,
            a.PrimaryIcd10Code,
            a.PrimaryIcd10Name,
            a.Status,
            a.TotalAdvancePayment,
            a.TotalInpatientExpenses,
            a.DischargeTime,
            a.DischargeDiagnosis,
            a.DischargeIcd10Code,
            a.Outcome,
            a.DischargeSummaryNotes,
            a.DischargeDoctorName,
            a.TransferHistories.Select(t => new BedTransferHistoryDto(
                t.Id, t.AdmissionId, t.FromBedId, t.FromBedCode, t.ToBedId, t.ToBedCode,
                t.FromDepartmentName, t.ToDepartmentName, t.TransferTime, t.Reason, t.TransferredBy
            )).ToList(),
            a.MedicalOrders.OrderByDescending(m => m.OrderDate).Select(m => new MedicalOrderDto(
                m.Id, m.AdmissionId, m.OrderDate, m.DoctorName, m.Diagnosis, m.CareLevel,
                m.DietPlan, m.MedicinesContent, m.ServicesContent, m.SpecialInstructions,
                m.IsExecuted, m.ExecutedByNurse, m.ExecutedTime
            )).ToList(),
            a.VitalSigns.OrderByDescending(v => v.RecordTime).Select(v => new VitalSignDto(
                v.Id, v.AdmissionId, v.RecordTime, v.Pulse, v.Temperature, v.BloodPressureSystolic,
                v.BloodPressureDiastolic, v.BreathingRate, v.SpO2, v.Weight, v.Notes, v.RecordedByNurse
            )).ToList(),
            a.CareNotes.OrderByDescending(c => c.NoteTime).Select(c => new CareNoteDto(
                c.Id, c.AdmissionId, c.NoteTime, c.ClinicalEvolution, c.NursingActions, c.Evaluation, c.NurseName
            )).ToList(),
            a.AdvancePayments.OrderByDescending(p => p.PaymentDate).Select(p => new AdvancePaymentDto(
                p.Id, p.AdmissionId, p.ReceiptNumber, p.PaymentDate, p.Amount, p.PaymentMethod, p.CashierName, p.Notes
            )).ToList()
        );

        return Result.Success(dto);
    }
}

public record GetAdmissionDetailQuery(Guid Id) : IRequest<Result<InpatientAdmissionDto>>;

public class GetAdmissionDetailHandler(IInpatientDbContext dbContext)
    : IRequestHandler<GetAdmissionDetailQuery, Result<InpatientAdmissionDto>>
{
    public async Task<Result<InpatientAdmissionDto>> Handle(GetAdmissionDetailQuery request, CancellationToken cancellationToken)
    {
        return await CreateAdmissionHandler.GetAdmissionDetail(request.Id, dbContext, cancellationToken);
    }
}

public record GetActiveAdmissionsQuery(string? DepartmentCode = null) : IRequest<Result<List<InpatientAdmissionDto>>>;

public class GetActiveAdmissionsHandler(IInpatientDbContext dbContext)
    : IRequestHandler<GetActiveAdmissionsQuery, Result<List<InpatientAdmissionDto>>>
{
    public async Task<Result<List<InpatientAdmissionDto>>> Handle(GetActiveAdmissionsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Admissions.Where(a => a.Status == AdmissionStatus.Admitted);
        if (!string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            query = query.Where(a => a.DepartmentCode == request.DepartmentCode);
        }

        var list = await query
            .OrderByDescending(a => a.AdmissionTime)
            .Select(a => new InpatientAdmissionDto(
                a.Id, a.AdmissionCode, a.PatientId, a.PatientCode, a.PatientName,
                a.DateOfBirth, a.Gender, a.HealthInsuranceNumber, a.RelativeContact,
                a.AdmissionTime, a.DepartmentCode, a.DepartmentName, a.CurrentBedId,
                a.CurrentBedCode, a.AdmittingDoctorName, a.InitialDiagnosis,
                a.PrimaryIcd10Code, a.PrimaryIcd10Name, a.Status, a.TotalAdvancePayment,
                a.TotalInpatientExpenses, a.DischargeTime, a.DischargeDiagnosis,
                a.DischargeIcd10Code, a.Outcome, a.DischargeSummaryNotes, a.DischargeDoctorName,
                new List<BedTransferHistoryDto>(), new List<MedicalOrderDto>(),
                new List<VitalSignDto>(), new List<CareNoteDto>(), new List<AdvancePaymentDto>()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(list);
    }
}

public record DischargeAdmissionCommand(
    Guid AdmissionId,
    string DischargeDiagnosis,
    string DischargeIcd10Code,
    DischargeOutcome Outcome,
    string DischargeSummaryNotes,
    string DischargeDoctorName
) : IRequest<Result<bool>>;

public class DischargeAdmissionHandler(IInpatientDbContext dbContext)
    : IRequestHandler<DischargeAdmissionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DischargeAdmissionCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<bool>("Không tìm thấy hồ sơ bệnh án");

        admission.Status = AdmissionStatus.Discharged;
        admission.DischargeTime = DateTime.UtcNow;
        admission.DischargeDiagnosis = request.DischargeDiagnosis;
        admission.DischargeIcd10Code = request.DischargeIcd10Code;
        admission.Outcome = request.Outcome;
        admission.DischargeSummaryNotes = request.DischargeSummaryNotes;
        admission.DischargeDoctorName = request.DischargeDoctorName;

        // Giải phóng giường và chuyển sang dọn vệ sinh
        if (admission.CurrentBedId.HasValue)
        {
            var bed = await dbContext.Beds.FirstOrDefaultAsync(b => b.Id == admission.CurrentBedId.Value, cancellationToken);
            if (bed != null)
            {
                bed.Status = BedStatus.Cleaning;
                bed.CurrentAdmissionId = null;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
