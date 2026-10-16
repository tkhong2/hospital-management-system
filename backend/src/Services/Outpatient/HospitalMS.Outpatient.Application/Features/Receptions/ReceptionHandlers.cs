using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.Common.Interfaces;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Domain.Entities;
using HospitalMS.Outpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Outpatient.Application.Features.Receptions;

public record CreateReceptionCommand(
    Guid PatientId,
    string PatientCode,
    string PatientName,
    DateTime? DateOfBirth,
    string Gender,
    string? PhoneNumber,
    string? Address,
    string? HealthInsuranceNumber,
    Guid ClinicRoomId,
    string ClinicRoomName,
    PatientType PatientType,
    string? Reason,
    int Priority = 0
) : IRequest<Result<ReceptionDto>>;

public class CreateReceptionValidator : AbstractValidator<CreateReceptionCommand>
{
    public CreateReceptionValidator()
    {
        RuleFor(x => x.PatientCode).NotEmpty().WithMessage("Mã bệnh nhân không được để trống");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Tên bệnh nhân không được để trống");
        RuleFor(x => x.ClinicRoomName).NotEmpty().WithMessage("Phòng khám không được để trống");
    }
}

public class CreateReceptionHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<CreateReceptionCommand, Result<ReceptionDto>>
{
    public async Task<Result<ReceptionDto>> Handle(CreateReceptionCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var maxQueue = await dbContext.Receptions
            .Where(r => r.ClinicRoomId == request.ClinicRoomId && r.CreatedAt >= today)
            .MaxAsync(r => (int?)r.QueueNumber, cancellationToken) ?? 0;

        var reception = new OutpatientReception
        {
            QueueNumber = maxQueue + 1,
            PatientId = request.PatientId,
            PatientCode = request.PatientCode,
            PatientName = request.PatientName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            HealthInsuranceNumber = request.HealthInsuranceNumber,
            ClinicRoomId = request.ClinicRoomId,
            ClinicRoomName = request.ClinicRoomName,
            PatientType = request.PatientType,
            Status = ReceptionStatus.Waiting,
            ReceptionTime = DateTime.UtcNow,
            Reason = request.Reason,
            Priority = request.Priority
        };

        dbContext.Receptions.Add(reception);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new ReceptionDto(
            reception.Id,
            reception.QueueNumber,
            reception.PatientId,
            reception.PatientCode,
            reception.PatientName,
            reception.DateOfBirth,
            reception.Gender,
            reception.PhoneNumber,
            reception.HealthInsuranceNumber,
            reception.ClinicRoomId,
            reception.ClinicRoomName,
            reception.PatientType,
            reception.Status,
            reception.ReceptionTime,
            reception.Reason,
            reception.Priority,
            null
        );

        return Result.Success(dto);
    }
}

public record GetReceptionQueueQuery(
    Guid? ClinicRoomId = null,
    ReceptionStatus? Status = null
) : IRequest<Result<List<ReceptionDto>>>;

public class GetReceptionQueueHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<GetReceptionQueueQuery, Result<List<ReceptionDto>>>
{
    public async Task<Result<List<ReceptionDto>>> Handle(GetReceptionQueueQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var query = dbContext.Receptions
            .Include(r => r.Encounter)
            .Where(r => r.CreatedAt >= today);

        if (request.ClinicRoomId.HasValue && request.ClinicRoomId.Value != Guid.Empty)
        {
            query = query.Where(r => r.ClinicRoomId == request.ClinicRoomId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var list = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.QueueNumber)
            .Select(r => new ReceptionDto(
                r.Id,
                r.QueueNumber,
                r.PatientId,
                r.PatientCode,
                r.PatientName,
                r.DateOfBirth,
                r.Gender,
                r.PhoneNumber,
                r.HealthInsuranceNumber,
                r.ClinicRoomId,
                r.ClinicRoomName,
                r.PatientType,
                r.Status,
                r.ReceptionTime,
                r.Reason,
                r.Priority,
                r.Encounter != null ? r.Encounter.Id : null
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(list);
    }
}

