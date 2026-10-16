using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.Common.Interfaces;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Domain.Entities;
using HospitalMS.Outpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Outpatient.Application.Features.Encounters;

public record StartEncounterCommand(
    Guid ReceptionId,
    string? DoctorId,
    string? DoctorName
) : IRequest<Result<EncounterDto>>;

public class StartEncounterHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<StartEncounterCommand, Result<EncounterDto>>
{
    public async Task<Result<EncounterDto>> Handle(StartEncounterCommand request, CancellationToken cancellationToken)
    {
        var reception = await dbContext.Receptions
            .Include(r => r.Encounter)
            .FirstOrDefaultAsync(r => r.Id == request.ReceptionId, cancellationToken);

        if (reception == null)
            return Result.Failure<EncounterDto>("Không tìm thấy lượt tiếp đón");

        if (reception.Encounter != null)
        {
            return await GetEncounterDto(reception.Encounter.Id, dbContext, cancellationToken);
        }

        reception.Status = ReceptionStatus.InConsultation;

        var encounter = new OutpatientEncounter
        {
            ReceptionId = reception.Id,
            PatientId = reception.PatientId,
            DoctorId = request.DoctorId,
            DoctorName = request.DoctorName,
            ClinicRoomName = reception.ClinicRoomName,
            StartTime = DateTime.UtcNow,
            ChiefComplaint = reception.Reason ?? "Khám bệnh định kỳ/theo yêu cầu",
            PrimaryIcd10Code = "Z00.0",
            PrimaryIcd10Name = "Khám sức khỏe tổng quát",
            Disposition = TreatmentDisposition.DischargedWithPrescription
        };

        dbContext.Encounters.Add(encounter);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetEncounterDto(encounter.Id, dbContext, cancellationToken);
    }

    private static async Task<Result<EncounterDto>> GetEncounterDto(Guid id, IOutpatientDbContext db, CancellationToken ct)
    {
        var e = await db.Encounters
            .Include(x => x.Reception)
            .Include(x => x.ServiceOrders).ThenInclude(s => s.Details)
            .Include(x => x.Prescriptions).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (e == null) return Result.Failure<EncounterDto>("Không tìm thấy phiên khám");

        var dto = new EncounterDto(
            e.Id,
            e.ReceptionId,
            e.PatientId,
            e.Reception?.PatientCode ?? "",
            e.Reception?.PatientName ?? "",
            e.DoctorId,
            e.DoctorName,
            e.ClinicRoomName,
            e.StartTime,
            e.EndTime,
            e.ChiefComplaint,
            e.HistoryOfPresentIllness,
            e.MedicalHistory,
            e.PhysicalExam,
            e.Pulse,
            e.BloodPressureSystolic,
            e.BloodPressureDiastolic,
            e.Temperature,
            e.SpO2,
            e.Weight,
            e.Height,
            e.PrimaryIcd10Code,
            e.PrimaryIcd10Name,
            e.SecondaryIcd10Code,
            e.SecondaryIcd10Name,
            e.ClinicalDiagnosis,
            e.Disposition,
            e.DoctorNotes,
            e.FollowUpDate,
            e.ServiceOrders.Select(so => new ServiceOrderDto(
                so.Id, so.EncounterId, so.OrderCode, so.OrderDate, so.DoctorName, so.Status,
                so.TotalAmount, so.TotalInsuranceAmount, so.TotalPatientAmount,
                so.Details.Select(d => new ServiceOrderDetailDto(
                    d.Id, d.ServiceId, d.ServiceCode, d.ServiceName, d.ServiceGroup,
                    d.Quantity, d.UnitPrice, d.InsuranceCoverageRate, d.InsuranceCoveredAmount,
                    d.PatientPayAmount, d.RoomToExecute, d.ResultSummary, d.ResultTime, d.IsAbnormal
                )).ToList()
            )).ToList(),
            e.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.EncounterId, p.PrescriptionCode, p.PrescriptionDate, p.DoctorName,
                p.Diagnosis, p.DaysCount, p.Advice, p.Status,
                p.TotalAmount, p.TotalInsuranceAmount, p.TotalPatientAmount,
                p.Items.Select(i => new PrescriptionItemDto(
                    i.Id, i.MedicineId, i.MedicineCode, i.MedicineName, i.ActiveIngredient,
                    i.DosageForm, i.Quantity, i.MorningDose, i.NoonDose, i.AfternoonDose,
                    i.NightDose, i.UsageInstructions, i.UnitPrice, i.IsInsuranceCovered,
                    i.InsuranceCoveredAmount, i.PatientPayAmount
                )).ToList()
            )).ToList()
        );

        return Result.Success(dto);
    }
}

public record UpdateEncounterDiagnosisCommand(
    Guid EncounterId,
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
    DateTime? FollowUpDate
) : IRequest<Result<EncounterDto>>;

public class UpdateEncounterDiagnosisHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<UpdateEncounterDiagnosisCommand, Result<EncounterDto>>
{
    public async Task<Result<EncounterDto>> Handle(UpdateEncounterDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var encounter = await dbContext.Encounters
            .Include(e => e.Reception)
            .Include(e => e.ServiceOrders).ThenInclude(s => s.Details)
            .Include(e => e.Prescriptions).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(e => e.Id == request.EncounterId, cancellationToken);

        if (encounter == null)
            return Result.Failure<EncounterDto>("Không tìm thấy phiên khám");

        encounter.ChiefComplaint = request.ChiefComplaint;
        encounter.HistoryOfPresentIllness = request.HistoryOfPresentIllness;
        encounter.MedicalHistory = request.MedicalHistory;
        encounter.PhysicalExam = request.PhysicalExam;
        encounter.Pulse = request.Pulse;
        encounter.BloodPressureSystolic = request.BloodPressureSystolic;
        encounter.BloodPressureDiastolic = request.BloodPressureDiastolic;
        encounter.Temperature = request.Temperature;
        encounter.SpO2 = request.SpO2;
        encounter.Weight = request.Weight;
        encounter.Height = request.Height;

        encounter.PrimaryIcd10Code = request.PrimaryIcd10Code;
        encounter.PrimaryIcd10Name = request.PrimaryIcd10Name;
        encounter.SecondaryIcd10Code = request.SecondaryIcd10Code;
        encounter.SecondaryIcd10Name = request.SecondaryIcd10Name;
        encounter.ClinicalDiagnosis = request.ClinicalDiagnosis;
        encounter.Disposition = request.Disposition;
        encounter.DoctorNotes = request.DoctorNotes;
        encounter.FollowUpDate = request.FollowUpDate;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new EncounterDto(
            encounter.Id,
            encounter.ReceptionId,
            encounter.PatientId,
            encounter.Reception?.PatientCode ?? "",
            encounter.Reception?.PatientName ?? "",
            encounter.DoctorId,
            encounter.DoctorName,
            encounter.ClinicRoomName,
            encounter.StartTime,
            encounter.EndTime,
            encounter.ChiefComplaint,
            encounter.HistoryOfPresentIllness,
            encounter.MedicalHistory,
            encounter.PhysicalExam,
            encounter.Pulse,
            encounter.BloodPressureSystolic,
            encounter.BloodPressureDiastolic,
            encounter.Temperature,
            encounter.SpO2,
            encounter.Weight,
            encounter.Height,
            encounter.PrimaryIcd10Code,
            encounter.PrimaryIcd10Name,
            encounter.SecondaryIcd10Code,
            encounter.SecondaryIcd10Name,
            encounter.ClinicalDiagnosis,
            encounter.Disposition,
            encounter.DoctorNotes,
            encounter.FollowUpDate,
            new List<ServiceOrderDto>(),
            new List<PrescriptionDto>()
        ));
    }
}

public record CompleteEncounterCommand(Guid EncounterId) : IRequest<Result<bool>>;

public class CompleteEncounterHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<CompleteEncounterCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteEncounterCommand request, CancellationToken cancellationToken)
    {
        var encounter = await dbContext.Encounters
            .Include(e => e.Reception)
            .FirstOrDefaultAsync(e => e.Id == request.EncounterId, cancellationToken);

        if (encounter == null)
            return Result.Failure<bool>("Không tìm thấy phiên khám");

        encounter.EndTime = DateTime.UtcNow;
        if (encounter.Reception != null)
        {
            encounter.Reception.Status = ReceptionStatus.Completed;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}

public record GetEncounterDetailQuery(Guid Id) : IRequest<Result<EncounterDto>>;

public class GetEncounterDetailHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<GetEncounterDetailQuery, Result<EncounterDto>>
{
    public async Task<Result<EncounterDto>> Handle(GetEncounterDetailQuery request, CancellationToken cancellationToken)
    {
        var e = await dbContext.Encounters
            .Include(x => x.Reception)
            .Include(x => x.ServiceOrders).ThenInclude(s => s.Details)
            .Include(x => x.Prescriptions).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (e == null)
            return Result.Failure<EncounterDto>("Không tìm thấy phiên khám");

        var dto = new EncounterDto(
            e.Id,
            e.ReceptionId,
            e.PatientId,
            e.Reception?.PatientCode ?? "",
            e.Reception?.PatientName ?? "",
            e.DoctorId,
            e.DoctorName,
            e.ClinicRoomName,
            e.StartTime,
            e.EndTime,
            e.ChiefComplaint,
            e.HistoryOfPresentIllness,
            e.MedicalHistory,
            e.PhysicalExam,
            e.Pulse,
            e.BloodPressureSystolic,
            e.BloodPressureDiastolic,
            e.Temperature,
            e.SpO2,
            e.Weight,
            e.Height,
            e.PrimaryIcd10Code,
            e.PrimaryIcd10Name,
            e.SecondaryIcd10Code,
            e.SecondaryIcd10Name,
            e.ClinicalDiagnosis,
            e.Disposition,
            e.DoctorNotes,
            e.FollowUpDate,
            e.ServiceOrders.Select(so => new ServiceOrderDto(
                so.Id, so.EncounterId, so.OrderCode, so.OrderDate, so.DoctorName, so.Status,
                so.TotalAmount, so.TotalInsuranceAmount, so.TotalPatientAmount,
                so.Details.Select(d => new ServiceOrderDetailDto(
                    d.Id, d.ServiceId, d.ServiceCode, d.ServiceName, d.ServiceGroup,
                    d.Quantity, d.UnitPrice, d.InsuranceCoverageRate, d.InsuranceCoveredAmount,
                    d.PatientPayAmount, d.RoomToExecute, d.ResultSummary, d.ResultTime, d.IsAbnormal
                )).ToList()
            )).ToList(),
            e.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.EncounterId, p.PrescriptionCode, p.PrescriptionDate, p.DoctorName,
                p.Diagnosis, p.DaysCount, p.Advice, p.Status,
                p.TotalAmount, p.TotalInsuranceAmount, p.TotalPatientAmount,
                p.Items.Select(i => new PrescriptionItemDto(
                    i.Id, i.MedicineId, i.MedicineCode, i.MedicineName, i.ActiveIngredient,
                    i.DosageForm, i.Quantity, i.MorningDose, i.NoonDose, i.AfternoonDose,
                    i.NightDose, i.UsageInstructions, i.UnitPrice, i.IsInsuranceCovered,
                    i.InsuranceCoveredAmount, i.PatientPayAmount
                )).ToList()
            )).ToList()
        );

        return Result.Success(dto);
    }
}

