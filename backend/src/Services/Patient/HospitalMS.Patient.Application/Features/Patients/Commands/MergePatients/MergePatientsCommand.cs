using System.Text.Json;
using FluentValidation;
using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Domain.Entities;
using HospitalMS.Patient.Domain.Enums;
using HospitalMS.Patient.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Commands.MergePatients;

public record MergePatientsCommand(
    Guid SourcePatientId,
    Guid TargetPatientId,
    string Reason
) : IRequest<Result>;

public class MergePatientsCommandValidator : AbstractValidator<MergePatientsCommand>
{
    public MergePatientsCommandValidator()
    {
        RuleFor(x => x.SourcePatientId).NotEmpty().WithMessage("Mã hồ sơ phụ (cần gộp) không được để trống.");
        RuleFor(x => x.TargetPatientId).NotEmpty().WithMessage("Mã hồ sơ chính (giữ lại) không được để trống.");
        RuleFor(x => x)
            .Must(x => x.SourcePatientId != x.TargetPatientId)
            .WithMessage("Hồ sơ cần gộp và hồ sơ chính không được trùng nhau.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Lý do hợp nhất hồ sơ bệnh nhân không được để trống.")
            .MaximumLength(500);
    }
}

public class MergePatientsCommandHandler(
    IPatientDbContext dbContext,
    IEventBus eventBus,
    ICurrentUser currentUser)
    : IRequestHandler<MergePatientsCommand, Result>
{
    public async Task<Result> Handle(MergePatientsCommand request, CancellationToken cancellationToken)
    {
        var source = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == request.SourcePatientId && !p.IsDeleted, cancellationToken);

        if (source is null)
        {
            return Result.Failure("Không tìm thấy hồ sơ bệnh nhân cần gộp (nguồn).");
        }

        if (source.IsMerged)
        {
            return Result.Failure($"Hồ sơ nguồn ({source.PatientCode}) đã được hợp nhất trước đó vào hồ sơ khác.");
        }

        var target = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == request.TargetPatientId && !p.IsDeleted, cancellationToken);

        if (target is null)
        {
            return Result.Failure("Không tìm thấy hồ sơ bệnh nhân chính (đích).");
        }

        if (target.IsMerged)
        {
            return Result.Failure($"Hồ sơ chính ({target.PatientCode}) đang là một hồ sơ đã bị gộp, vui lòng chọn hồ sơ còn hiệu lực.");
        }

        // 1. Snapshot thông tin hồ sơ nguồn trước khi gộp
        var snapshotJson = JsonSerializer.Serialize(source);

        var history = new PatientMergeHistory
        {
            SourcePatientId = source.Id,
            SourcePatientCode = source.PatientCode,
            TargetPatientId = target.Id,
            TargetPatientCode = target.PatientCode,
            MergeReason = request.Reason.Trim(),
            MergedBy = currentUser.UserName ?? "system",
            MergedAt = DateTime.UtcNow,
            SnapshotDataJson = snapshotJson
        };

        // 2. Điền bổ sung các thông tin còn thiếu từ source sang target nếu target chưa có
        if (string.IsNullOrWhiteSpace(target.IdentityCardNumber) && !string.IsNullOrWhiteSpace(source.IdentityCardNumber))
        {
            target.IdentityCardNumber = source.IdentityCardNumber;
        }

        if (string.IsNullOrWhiteSpace(target.HealthInsuranceNumber) && !string.IsNullOrWhiteSpace(source.HealthInsuranceNumber))
        {
            target.HealthInsuranceNumber = source.HealthInsuranceNumber;
        }

        if (string.IsNullOrWhiteSpace(target.PhoneNumber) && !string.IsNullOrWhiteSpace(source.PhoneNumber))
        {
            target.PhoneNumber = source.PhoneNumber;
        }

        if (string.IsNullOrWhiteSpace(target.Email) && !string.IsNullOrWhiteSpace(source.Email))
        {
            target.Email = source.Email;
        }

        if (target.BloodType == BloodType.Unknown && source.BloodType != BloodType.Unknown)
        {
            target.BloodType = source.BloodType;
        }

        if (string.IsNullOrWhiteSpace(target.Allergies) && !string.IsNullOrWhiteSpace(source.Allergies))
        {
            target.Allergies = source.Allergies;
        }

        // 3. Đánh dấu hồ sơ nguồn đã gộp
        source.MarkAsMerged(target.Id, request.Reason.Trim());

        dbContext.PatientMergeHistories.Add(history);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Phát Integration Event thông báo gộp hồ sơ cho toàn hệ thống
        var mergeEvent = new PatientMergedIntegrationEvent(
            source.Id,
            source.PatientCode,
            target.Id,
            target.PatientCode,
            request.Reason.Trim()
        );

        await eventBus.PublishAsync(mergeEvent, cancellationToken);

        return Result.Success();
    }
}

